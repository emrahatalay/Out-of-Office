"""Altını kurar: temiz, az üçgenli disk + koddan çizilen yüz dokusu.

Meshy'nin altını kullanılmaz: 300 üçgende dokusu parçalanıyor, amblem kayboluyor.
Doku da konseptten kesilmez (soluk krem çıkıyor, amblem okunmuyordu): doygun altın tonlarında,
ışığı dokuya gömülü (üstten aydınlık, alttan gölge) kabartma kenar ve çip amblemi çizilir.
Kullanım: blender -b --factory-startup --python make_coin.py -- <çıktı klasörü>
"""
import math
import os
import sys

import bpy
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pipeline_common as common  # noqa: E402

out_dir = sys.argv[sys.argv.index("--") + 1:][0]
NAME = "Coin"
SEGMENTS = 20
RADIUS, FACE_RADIUS = 0.35, 0.31     # dış kenar ve yüzün düz kısmı (m)
HALF_EDGE, HALF_FACE = 0.03, 0.045   # kenarda ve yüzde yarı kalınlık (m)
CENTER_Z = 1.0                       # oyunda altının merkez yüksekliği
TEXTURE = 256
SUPERSAMPLE = 4                      # kenarlar yumuşasın diye büyük çizilip küçültülür
GLOW = 0.22                          # altın çok hafif ışır (fazlası bloom'la beyaza kaçar)

# Renkler (doğrusal değil, sRGB 0-1). Koyu kahve dış çizgi oyunun "kalın koyu çizgi" diline uyar.
INK = np.array([0.42, 0.20, 0.02])
DEEP = np.array([0.86, 0.46, 0.04])
GOLD = np.array([0.99, 0.69, 0.10])
LIGHT = np.array([1.00, 0.86, 0.36])
SHINE = np.array([1.00, 0.97, 0.78])

bpy.ops.wm.read_factory_settings(use_empty=True)


def smooth(edge0, edge1, x):
    t = np.clip((x - edge0) / (edge1 - edge0), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def mix(a, b, t):
    return a * (1 - t[..., None]) + b * t[..., None]


def box(px, py, half_w, half_h, radius):
    """Köşesi yuvarlak kutunun işaretli uzaklığı."""
    qx = np.abs(px) - half_w + radius
    qy = np.abs(py) - half_h + radius
    outside = np.hypot(np.maximum(qx, 0), np.maximum(qy, 0))
    return outside + np.minimum(np.maximum(qx, qy), 0) - radius


def emblem(px, py):
    """Çip: yuvarlak köşeli kare, her yanda üç bacak, ortada küçük çekirdek. Uzaklık döner."""
    body = box(px, py, 0.105, 0.105, 0.03)
    pins = np.full_like(px, 1e3)
    for k in (-0.055, 0.0, 0.055):
        pins = np.minimum(pins, box(px - k, py, 0.016, 0.15, 0.012))
        pins = np.minimum(pins, box(px, py - k, 0.15, 0.016, 0.012))
    return np.minimum(body, pins)


# ---- Doku. UV'de yüzün düz kısmı r < 0.47 * FACE_RADIUS / RADIUS, pah 0.47'ye kadar, dışı kenar şeridi.
n = TEXTURE * SUPERSAMPLE
g = (np.arange(n) + 0.5) / n - 0.5
x = g[None, :]
y = -g[:, None]                       # +y yukarı (dokunun üstü)
x, y = np.broadcast_arrays(x, y)
r = np.hypot(x, y)
face_r = 0.47 * FACE_RADIUS / RADIUS
light = np.clip(0.5 + 0.5 * (y * 0.8 - x * 0.35) / 0.47, 0, 1)   # üst-sol aydınlık

# Yüz: altın, üstte açık.
color = mix(DEEP, GOLD, smooth(0.0, 0.75, light))
color = mix(color, LIGHT, smooth(0.6, 1.0, light) * 0.6)

# İç oluk halkası: koyu çizgi, altında ince parlak çizgi (kabartma hissi).
groove = np.abs(r - face_r * 0.86)
color = mix(color, DEEP, smooth(0.016, 0.006, groove))
color = mix(color, INK, smooth(0.008, 0.003, groove) * 0.7)

# Amblem: kabartma çip. Altına gölge düşer, gövde açık altın, üst-sol kenarı parlar, koyu dış çizgi.
EMBLEM_SCALE = 1.45


def emblem_at(ox, oy):
    return emblem((x + ox) / EMBLEM_SCALE, (y + oy) / EMBLEM_SCALE) * EMBLEM_SCALE


d = emblem_at(0, 0)
shadow = smooth(0.006, -0.006, emblem_at(-0.014, 0.014))       # sağ-alta düşen gölge
color = mix(color, DEEP * 0.85, shadow * 0.75)
inside = smooth(0.004, -0.004, d)
body = mix(GOLD, LIGHT, smooth(0.2, 0.9, light))
color = mix(color, body, inside)
rim_light = inside * smooth(-0.002, 0.010, emblem_at(0.012, -0.012))  # üst-sol kenar
color = mix(color, SHINE, rim_light * 0.9)
core = box(x, y, 0.06, 0.06, 0.016)
color = mix(color, DEEP, smooth(0.004, -0.004, core))
color = mix(color, LIGHT, smooth(0.004, -0.004, box(x + 0.012, y - 0.012, 0.03, 0.03, 0.01)) * 0.5)
color = mix(color, INK, smooth(0.016, 0.004, np.abs(d)))
color = mix(color, INK, smooth(0.012, 0.002, np.abs(core)) * 0.9)

# Pah (dış halka): üstte parlak, altta koyu; iki yanında koyu çizgi.
bevel = smooth(face_r - 0.004, face_r + 0.004, r)
bevel_color = mix(DEEP, LIGHT, smooth(0.15, 0.85, light))
bevel_color = mix(bevel_color, SHINE, smooth(0.8, 1.0, light) * 0.7)
color = mix(color, bevel_color, bevel)
color = mix(color, INK, smooth(0.010, 0.0, np.abs(r - face_r)) * 0.9)
color = mix(color, INK, smooth(0.47 - 0.025, 0.47, r) * 0.75)   # yüzün dış çizgisi
# Kenar şeridi: yandan bakınca altın görünsün diye parlak altın (koyu olursa turuncu / bronz okunur).
rim = mix(GOLD, DEEP, np.full_like(r, 0.3))
color = np.where((r > 0.47)[..., None], rim, color)

# Küçült (kutu süzgeci).
color = color.reshape(TEXTURE, SUPERSAMPLE, TEXTURE, SUPERSAMPLE, 3).mean(axis=(1, 3))

pixels = np.empty((TEXTURE, TEXTURE, 4), dtype=np.float32)
pixels[..., :3] = color[::-1]          # Blender görüntüsünün ilk satırı alttır
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
