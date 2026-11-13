"""Rig'li karakter FBX'inin gömülü dokusunu oyuna hazırlar: boşluk doldurma, 1024'e küçültme, ışıma maskesi.

Karakterin FBX'i Meshy'den geldiği gibi kullanılır (iskelet ve ağırlıklar bozulmasın); yalnızca doku
ayrı dosyaya çıkarılır.
Kullanım: blender -b --factory-startup --python convert_character_texture.py -- <props.json> <Ad> <rig.fbx> <çıktı.png>
"""
import json
import os
import sys

import bpy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import pipeline_common as common  # noqa: E402

manifest_path, name, source, out_path = sys.argv[sys.argv.index("--") + 1:][:4]
spec = json.load(open(manifest_path, encoding="utf-8"))["characters"][name]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=source)
mesh = next(o for o in bpy.context.scene.objects if o.type == 'MESH').data
image = common.first_packed_image()
glow = common.save_texture(image, out_path, spec.get("texture", 1024), spec.get("glow"), mesh)
print("RESULT %s glow=%.3f" % (name, glow))
