"""Model hattının ortak işlevleri. Blender'ın Python'unda çalışır (bpy, numpy)."""
import bpy
import numpy as np


def rgb_to_hsv(rgb):
    """rgb: (..., 3) 0-1. Dönen: ton (derece), doygunluk, parlaklık."""
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    v = rgb.max(axis=-1)
    lo = rgb.min(axis=-1)
    delta = v - lo
    s = np.where(v > 1e-5, delta / np.maximum(v, 1e-5), 0.0)
    safe = np.maximum(delta, 1e-5)
    h = np.where(v == r, (g - b) / safe % 6.0, np.where(v == g, (b - r) / safe + 2.0, (r - g) / safe + 4.0))
    return h * 60.0 % 360.0, s, v


def smoothstep(lo, hi, x):
    t = np.clip((x - lo) / max(hi - lo, 1e-5), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def glow_mask(rgb, rule):
    """Neon bölgeleri bulur: parlak ve doygun pikseller, istenirse yalnızca verilen ton aralıklarında.

    rule: {"v": [alt, üst], "s": [alt, üst], "hues": [[baş, son], ...], "gain": çarpan}
    Dönen maske 0-1 (1 = tam ışır).
    """
    if not rule:
        return np.zeros(rgb.shape[:-1], dtype=np.float32)

    h, s, v = rgb_to_hsv(rgb)
    hues = rule.get("hues")
    # Neonun ortası beyaza yakın (soluk) boyanır; ton verilmişse doygunluk eşiği düşük tutulur.
    v_lo, v_hi = rule.get("v", [0.75, 0.95])
    s_lo, s_hi = rule.get("s", [0.2, 0.45] if hues else [0.45, 0.75])
    mask = smoothstep(v_lo, v_hi, v) * smoothstep(s_lo, s_hi, s)

    if hues:
        inside = np.zeros_like(mask, dtype=bool)
        for start, end in hues:
            inside |= ((h >= start) & (h <= end)) if start <= end else ((h >= start) | (h <= end))
        mask = np.where(inside, mask, 0.0)

    return np.clip(mask * rule.get("gain", 1.0), 0.0, 1.0).astype(np.float32)


def uv_coverage(mesh, size):
    """UV üçgenlerinin kapladığı pikseller (True). Dokunun kullanılmayan (siyah) boşluklarını ayırır."""
    covered = np.zeros((size, size), dtype=bool)
    uv = mesh.uv_layers.active.data
    for polygon in mesh.polygons:
        loops = list(polygon.loop_indices)
        base = np.array(uv[loops[0]].uv) * size
        for i in range(1, len(loops) - 1):
            tri = np.array([base, np.array(uv[loops[i]].uv) * size, np.array(uv[loops[i + 1]].uv) * size])
            x0, y0 = np.floor(tri.min(axis=0)).astype(int) - 1
            x1, y1 = np.ceil(tri.max(axis=0)).astype(int) + 1
            x0, y0, x1, y1 = max(x0, 0), max(y0, 0), min(x1, size), min(y1, size)
            if x1 <= x0 or y1 <= y0:
                continue
            xs, ys = np.meshgrid(np.arange(x0, x1) + 0.5, np.arange(y0, y1) + 0.5)
            inside = np.ones(xs.shape, dtype=bool)
            area = (tri[1][0] - tri[0][0]) * (tri[2][1] - tri[0][1]) - (tri[1][1] - tri[0][1]) * (tri[2][0] - tri[0][0])
            sign = 1.0 if area >= 0 else -1.0
            for k in range(3):
                p, q = tri[k], tri[(k + 1) % 3]
                edge = q - p
                length = max(float(np.hypot(edge[0], edge[1])), 1e-6)
                # Kenara yarım piksel pay: sınırdaki pikseller de kaplanmış sayılır.
                inside &= sign * (edge[0] * (ys - p[1]) - edge[1] * (xs - p[0])) / length >= -0.75
            covered[y0:y1, x0:x1] |= inside
    return covered


def fill_gaps(rgb, covered):
    """Kaplanmamış pikselleri çevredeki renklerle doldurur (push-pull). Mipmap ve küçültmede
    UV adalarının kenarına siyah sızmasını önler."""
    levels = [(rgb * covered[..., None], covered.astype(np.float32))]
    while levels[-1][1].shape[0] > 1:
        color, weight = levels[-1]
        n = color.shape[0] // 2
        color = color.reshape(n, 2, n, 2, 3).sum(axis=(1, 3))
        weight = weight.reshape(n, 2, n, 2).sum(axis=(1, 3))
        levels.append((color, weight))

    filled = levels[-1][0] / np.maximum(levels[-1][1][..., None], 1e-6)
    for color, weight in reversed(levels[:-1]):
        up = filled.repeat(2, axis=0).repeat(2, axis=1)
        own = color / np.maximum(weight[..., None], 1e-6)
        filled = np.where(weight[..., None] > 0, own, up)
    return filled


def save_texture(image, path, size, rule, mesh=None):
    """Dokuyu hazırlar ve PNG kaydeder: boşluk doldurma, küçültme, ışıma maskesi.

    Işıma maskesi alfaya yazılır: alfa 1 = ışımaz, alfa 0 = tam ışır.
    """
    source = image.size[0]
    pixels = np.empty(source * source * 4, dtype=np.float32)
    image.pixels.foreach_get(pixels)
    pixels = pixels.reshape(source, source, 4)
    rgb = pixels[..., :3]

    if mesh is not None and mesh.uv_layers.active is not None:
        rgb = fill_gaps(rgb, uv_coverage(mesh, source))

    # Kutu ortalamasıyla küçültme (kaynak ve hedef ikinin kuvvetidir).
    factor = source // size
    if factor > 1:
        rgb = rgb.reshape(size, factor, size, factor, 3).mean(axis=(1, 3))

    out_pixels = np.empty((size, size, 4), dtype=np.float32)
    out_pixels[..., :3] = rgb
    mask = glow_mask(rgb, rule)
    out_pixels[..., 3] = 1.0 - mask
    write_png(out_pixels, path)
    return float(mask.mean())


def write_png(pixels, path):
    size = pixels.shape[0]
    out = bpy.data.images.new("out", size, size, alpha=True)
    out.alpha_mode = 'STRAIGHT'
    out.pixels.foreach_set(np.ascontiguousarray(pixels, dtype=np.float32).ravel())
    out.filepath_raw = path
    out.file_format = 'PNG'
    out.save()
    bpy.data.images.remove(out)


def first_packed_image():
    for image in bpy.data.images:
        if image.size[0] > 0 and image.name not in ("Render Result", "Viewer Node"):
            return image
    return None
