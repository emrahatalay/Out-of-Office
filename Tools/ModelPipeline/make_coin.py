"""Altını kurar: temiz, az üçgenli disk + konsept görselden kesilen yüz dokusu.

Meshy'nin altını kullanılmaz: 300 üçgende dokusu parçalanıyor, amblem kayboluyor.
Kullanım: blender -b --factory-startup --python make_coin.py -- <konsept.png> <çıktı klasörü>
"""
import math
import os
import sys

import bpy
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pipeline_common as common  # noqa: E402

concept_path, out_dir = sys.argv[sys.argv.index("--") + 1:][:2]
NAME = "Coin"
SEGMENTS = 20
RADIUS, FACE_RADIUS = 0.35, 0.31     # dış kenar ve yüzün düz kısmı (m)
HALF_EDGE, HALF_FACE = 0.03, 0.045   # kenarda ve yüzde yarı kalınlık (m)
CENTER_Z = 1.0                       # oyunda altının merkez yüksekliği
TEXTURE = 128
FACE_SHARE = 0.86                    # konseptte altın hafif dönük: sınır kutusunun solundaki bu pay yüzdür
GLOW = 0.45                          # tüm altın hafifçe ışır

bpy.ops.wm.read_factory_settings(use_empty=True)

# ---- Doku: konseptteki altının yüzünü kes, kareye ger, daire dışını kenar rengiyle doldur.
concept = bpy.data.images.load(concept_path)
w, h = concept.size
px = np.empty(w * h * 4, dtype=np.float32)
concept.pixels.foreach_get(px)
px = px.reshape(h, w, 4)[..., :3]
hue, sat, val = common.rgb_to_hsv(px)
gold = (sat > 0.45) & (hue > 25) & (hue < 65) & (val > 0.45)
ys, xs = np.nonzero(gold)
x0, x1, y0, y1 = xs.min(), xs.max(), ys.min(), ys.max()
x1 = int(x0 + (x1 - x0) * FACE_SHARE)
crop = px[y0:y1, x0:x1]
iy = (np.arange(TEXTURE) + 0.5) / TEXTURE * crop.shape[0]
ix = (np.arange(TEXTURE) + 0.5) / TEXTURE * crop.shape[1]
face = crop[iy.astype(int)][:, ix.astype(int)]

grid = (np.arange(TEXTURE) + 0.5) / TEXTURE - 0.5
dist = np.hypot(grid[None, :], grid[:, None])
rim_color = face[(dist > 0.40) & (dist < 0.46)].mean(axis=0)
face = np.where((dist < 0.47)[..., None], face, rim_color)

pixels = np.empty((TEXTURE, TEXTURE, 4), dtype=np.float32)
pixels[..., :3] = face
pixels[..., 3] = 1.0 - GLOW
os.makedirs(out_dir, exist_ok=True)
common.write_png(pixels, os.path.join(out_dir, NAME + "_BaseColor.png"))

# ---- Mesh: iki yüz (ortası düz, kenarı pahlı) + kenar şeridi. Yüzler oyuncuya (Blender +Y) ve arkaya bakar.
verts, uvs, faces = [], [], []


def add(x, y, z, u, v):
    verts.append((x, y, z + CENTER_Z))
    uvs.append((u, v))
    return len(verts) - 1


def ring(radius, y, side, uv_radius):
    out = []
    for i in range(SEGMENTS):
        a = 2 * math.pi * i / SEGMENTS
        x, z = math.cos(a) * radius, math.sin(a) * radius
        # Ön yüz +Y'den bakılınca doğru okunsun diye U çevrilir.
        out.append(add(x, y, z, 0.5 - side * math.cos(a) * uv_radius, 0.5 + math.sin(a) * uv_radius))
    return out


for side in (1, -1):
    center = add(0, side * HALF_FACE, 0, 0.5, 0.5)
    inner = ring(FACE_RADIUS, side * HALF_FACE, side, 0.47 * FACE_RADIUS / RADIUS)
    outer = ring(RADIUS, side * HALF_EDGE, side, 0.47)
    for i in range(SEGMENTS):
        j = (i + 1) % SEGMENTS
        tri = (center, inner[j], inner[i]) if side > 0 else (center, inner[i], inner[j])
        faces.append(tri)
        quad = (inner[i], inner[j], outer[j], outer[i]) if side > 0 else (inner[j], inner[i], outer[i], outer[j])
        faces.append(quad)

# Kenar şeridi kendi köşeleriyle (sert kenar); dokuda dairenin dışındaki düz kenar rengine bakar.
front = [add(math.cos(2 * math.pi * i / SEGMENTS) * RADIUS, HALF_EDGE, math.sin(2 * math.pi * i / SEGMENTS) * RADIUS, 0.03, 0.03) for i in range(SEGMENTS)]
back = [add(math.cos(2 * math.pi * i / SEGMENTS) * RADIUS, -HALF_EDGE, math.sin(2 * math.pi * i / SEGMENTS) * RADIUS, 0.03, 0.03) for i in range(SEGMENTS)]
for i in range(SEGMENTS):
    j = (i + 1) % SEGMENTS
    faces.append((front[i], front[j], back[j], back[i]))

mesh = bpy.data.meshes.new(NAME)
mesh.from_pydata(verts, [], faces)
layer = mesh.uv_layers.new(name="UVMap")
for polygon in mesh.polygons:
    polygon.use_smooth = True
    for loop in polygon.loop_indices:
        layer.data[loop].uv = uvs[mesh.loops[loop].vertex_index]
mesh.update()

obj = bpy.data.objects.new(NAME, mesh)
bpy.context.scene.collection.objects.link(obj)
obj.select_set(True)
bpy.context.view_layer.objects.active = obj
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.normals_make_consistent(inside=False)
bpy.ops.mesh.quads_convert_to_tris()
bpy.ops.object.mode_set(mode='OBJECT')

bpy.ops.export_scene.fbx(
    filepath=os.path.join(out_dir, NAME + ".fbx"), use_selection=True, object_types={'MESH'},
    apply_scale_options='FBX_SCALE_ALL', bake_space_transform=True, mesh_smooth_type='OFF',
    bake_anim=False, add_leaf_bones=False, path_mode='STRIP', embed_textures=False)
print("RESULT %s tris=%d verts=%d" % (NAME, len(mesh.polygons), len(mesh.vertices)))
