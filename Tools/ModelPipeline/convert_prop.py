"""Meshy GLB'sini oyuna hazır hale getirir: yön, ölçek, orijin, dilimleme, doku + ışıma maskesi, dokusuz FBX.

Kullanım:
  blender -b --factory-startup --python convert_prop.py -- <props.json> <Ad> <kaynak.glb> <çıktı klasörü> [önizleme.png]

Eksenler (Blender): X genişlik, Y derinlik (-Y ön), Z yükseklik.
"""
import json
import math
import os
import sys

import bmesh
import bpy
from mathutils import Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pipeline_common as common  # noqa: E402

args = sys.argv[sys.argv.index("--") + 1:]
manifest_path, name, source, out_dir = args[:4]
preview_path = args[4] if len(args) > 4 else None
spec = json.load(open(manifest_path, encoding="utf-8"))["props"][name]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=source)

meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
for o in bpy.context.scene.objects:
    o.select_set(o.type == 'MESH')
bpy.context.view_layer.objects.active = meshes[0]
if len(meshes) > 1:
    bpy.ops.object.join()
obj = bpy.context.view_layer.objects.active
bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

image = common.first_packed_image()

# Yön. "yaw" önce modelin önünü Blender'da -Y'ye getirir (Meshy çoğu modeli zaten öyle verir).
# Sonra "face": "camera" (varsayılan) önü gelen oyuncuya, yani Unity'de -Z'ye çevirir;
# "forward" oyuncuyla aynı yöne (+Z) bırakır. Blender -Y, FBX'ten sonra Unity +Z olur.
yaw = spec.get("yaw", 0) + (180 if spec.get("face", "camera") == "camera" else 0)
if "PIPELINE_YAW" in os.environ:
    yaw = float(os.environ["PIPELINE_YAW"])
if yaw:
    obj.matrix_world = Matrix.Rotation(math.radians(yaw), 4, 'Z') @ obj.matrix_world
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def bounds():
    xs = [v.co for v in obj.data.vertices]
    lo = Vector((min(c.x for c in xs), min(c.y for c in xs), min(c.z for c in xs)))
    hi = Vector((max(c.x for c in xs), max(c.y for c in xs), max(c.z for c in xs)))
    return lo, hi


lo, hi = bounds()
size = hi - lo
fit = spec["fit"]
scales = []
if "height" in fit: scales.append(fit["height"] / size.z)
if "width" in fit: scales.append(fit["width"] / size.x)
if "depth" in fit: scales.append(fit["depth"] / size.y)
if "max" in fit: scales.append(fit["max"] / max(size))
scale = min(scales)
if "max_width" in fit: scale = min(scale, fit["max_width"] / size.x)
if "max_depth" in fit: scale = min(scale, fit["max_depth"] / size.y)
obj.scale = (scale, scale, scale)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

# Orijin: yatayda ortada; düşeyde ya taban "lift" yüksekliğinde ya da merkez "center_z" yüksekliğinde.
lo, hi = bounds()
center = (lo + hi) * 0.5
if "center_z" in spec:
    dz = spec["center_z"] - center.z
else:
    dz = spec.get("lift", 0.0) - lo.z
offset = Vector((-center.x, -center.y, dz))
for v in obj.data.vertices:
    v.co += offset
lo, hi = bounds()
size = hi - lo

# Kıvrımlı dünya yalnızca köşeleri büker: derin mesh'ler Y boyunca dilimlenir.
slice_step = spec.get("slice", 2.5)
if size.y > 4.0 and slice_step > 0:
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    y = lo.y + slice_step
    while y < hi.y - 0.2:
        geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
        bmesh.ops.bisect_plane(bm, geom=geom, plane_co=(0, y, 0), plane_no=(0, 1, 0))
        y += slice_step
    bmesh.ops.triangulate(bm, faces=bm.faces[:])
    bm.to_mesh(obj.data)
    bm.free()

tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
budget = spec.get("tris")
if budget and tris > budget * 1.2:
    modifier = obj.modifiers.new("Decimate", 'DECIMATE')
    modifier.ratio = budget / tris
    bpy.ops.object.modifier_apply(modifier="Decimate")
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)

os.makedirs(out_dir, exist_ok=True)
glow = 0.0
if image is not None:
    glow = common.save_texture(image, os.path.join(out_dir, name + "_BaseColor.png"), spec.get("texture", 512), spec.get("glow"), obj.data)

if preview_path:
    # Oyuncunun gördüğü yön: önden (-Y'den +Y'ye), hafif yukarıdan. Doku ışıksız, olduğu gibi.
    scene = bpy.context.scene
    material = obj.data.materials[0] if obj.data.materials else None
    if material and material.use_nodes:
        nodes, links = material.node_tree.nodes, material.node_tree.links
        tex = next((n for n in nodes if n.type == 'TEX_IMAGE'), None)
        out = next((n for n in nodes if n.type == 'OUTPUT_MATERIAL'), None)
        if tex and out:
            emission = nodes.new('ShaderNodeEmission')
            links.new(tex.outputs['Color'], emission.inputs['Color'])
            links.new(emission.outputs['Emission'], out.inputs['Surface'])
    reach = max(size) * 1.25
    cam_data = bpy.data.cameras.new("cam")
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = reach
    cam = bpy.data.objects.new("cam", cam_data)
    scene.collection.objects.link(cam)
    target = Vector((0, 0, (lo.z + hi.z) * 0.5))
    # Oyuncunun kamerası: Unity'de -Z'den +Z'ye bakar; Blender'da bu +Y'den -Y'ye bakmaktır.
    cam.location = target + Vector((-reach * 0.7, reach * 2.0, reach * 0.7))
    cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
    scene.camera = cam
    scene.render.engine = 'BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items] else 'BLENDER_EEVEE'
    scene.render.resolution_x = scene.render.resolution_y = 256
    scene.render.film_transparent = False
    scene.view_settings.view_transform = 'Standard'
    scene.world = bpy.data.worlds.new("w")
    scene.world.color = (0.05, 0.06, 0.1)
    scene.render.filepath = preview_path
    bpy.ops.render.render(write_still=True)

# Dokusuz, materyalsiz FBX: Unity materyali kendi kurar.
obj.data.materials.clear()
obj.name = name
obj.data.name = name
bpy.ops.object.select_all(action='DESELECT')
obj.select_set(True)
bpy.context.view_layer.objects.active = obj
bpy.ops.export_scene.fbx(
    filepath=os.path.join(out_dir, name + ".fbx"), use_selection=True, object_types={'MESH'},
    apply_scale_options='FBX_SCALE_ALL', bake_space_transform=True, mesh_smooth_type='OFF',
    use_mesh_modifiers=True, bake_anim=False, add_leaf_bones=False, path_mode='STRIP', embed_textures=False)

print("RESULT %s tris=%d verts=%d size=(%.2f w, %.2f d, %.2f h) z=%.2f..%.2f glow=%.3f" %
      (name, tris, len(obj.data.vertices), size.x, size.y, size.z, lo.z, hi.z, glow))
