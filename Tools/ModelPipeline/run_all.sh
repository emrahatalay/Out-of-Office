#!/bin/sh
# Tüm modelleri kaynak GLB'lerden yeniden üretir.
# Kullanım: run_all.sh <kaynak GLB klasörü> <çıktı klasörü> [önizleme klasörü]
# Kaynak dosya adı props.json'daki "source" alanıdır (<source>.glb).
DIR="$(cd "$(dirname "$0")" && pwd)"
SRC="$1"; OUT="$2"; PREV="$3"
python3 -c "import json,sys; [print(k, v['source']) for k, v in json.load(open(sys.argv[1], encoding='utf-8'))['props'].items()]" "$DIR/props.json" |
while read name source; do
  preview=""; [ -n "$PREV" ] && preview="$PREV/$name.png"
  blender -b --factory-startup --python "$DIR/convert_prop.py" -- "$DIR/props.json" "$name" "$SRC/$source.glb" "$OUT" $preview 2>&1 | grep -E "^RESULT|Traceback|Error:" 
done
