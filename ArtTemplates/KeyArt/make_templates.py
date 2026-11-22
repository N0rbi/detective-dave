import os, zipfile
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
HEIGHT_STRETCH = 2
SHAPES = [
    ("1x1", "1", 1.0, 1.0, False, "letters, digits"),
    ("1_25x1", "1.25", 1.25, 1.0, False, "a few edge keys"),
    ("1_5x1", "1.5", 1.5, 1.0, False, "Tab, Ctrl-size keys"),
    ("1_75x1", "1.75", 1.75, 1.0, False, "Caps Lock"),
    ("2x1", "2", 2.0, 1.0, False, "Backspace"),
    ("2_25x1", "2.25", 2.25, 1.0, False, "Shift, ANSI Enter"),
    ("2_75x1", "2.75", 2.75, 1.0, False, "right Shift"),
    ("enter_1_5x2", "Enter", 1.5, 2.0, True, "L-shaped Enter (ISO/JIS)"),
]
SETS = {"pixel16": 16, "hires64": 64}
GUIDE = (255, 64, 160, 255)
SAFE = (0, 200, 255, 255)
FILL = (150, 150, 150, 70)
CROSS = (255, 255, 255, 160)


def size_px(w, h, ppu):
    return round(w * ppu), round(h * ppu * HEIGHT_STRETCH)


def enter_mask(width, height, ppu):
    top = round(1 * ppu * HEIGHT_STRETCH)
    cut = round(0.25 * ppu)
    mask = Image.new("L", (width, height), 0)
    d = ImageDraw.Draw(mask)
    d.rectangle([0, 0, width - 1, top - 1], fill=255)
    d.rectangle([cut, top, width - 1, height - 1], fill=255)
    return mask


def body_mask(shape, ppu):
    _, _, w, h, whole, _ = shape
    width, height = size_px(w, h, ppu)
    return enter_mask(width, height, ppu) if whole else Image.new("L", (width, height), 255)


def draw_guides(shape, ppu):
    name, label, w, h, whole, used = shape
    width, height = size_px(w, h, ppu)
    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    mask = body_mask(shape, ppu)
    fill = Image.new("RGBA", (width, height), FILL)
    image.paste(fill, (0, 0), mask)
    d = ImageDraw.Draw(image)
    inset = max(1, round(ppu / 8))
    edge = Image.new("L", (width, height), 0)
    px, mp = edge.load(), mask.load()
    for y in range(height):
        for x in range(width):
            if mp[x, y] and any(not (0 <= x + dx < width and 0 <= y + dy < height) or not mp[x + dx, y + dy] for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                px[x, y] = 255
    image.paste(Image.new("RGBA", (width, height), GUIDE), (0, 0), edge)
    cx, cy = (width // 2, height // 2) if not whole else (width // 2 + round(0.125 * ppu), height // 2)
    arm = max(2, round(ppu / 4))
    d.line([cx - arm, cy, cx + arm, cy], fill=CROSS)
    d.line([cx, cy - arm, cx, cy + arm], fill=CROSS)
    if not whole:
        d.rectangle([inset, inset, width - 1 - inset, height - 1 - inset], outline=SAFE)
    else:
        top = round(1 * ppu * HEIGHT_STRETCH)
        cut = round(0.25 * ppu)
        pts = [(inset, inset), (width - 1 - inset, inset), (width - 1 - inset, height - 1 - inset), (cut + inset, height - 1 - inset), (cut + inset, top - inset), (inset, top - inset), (inset, inset)]
        d.line(pts, fill=SAFE)
    if ppu >= 32:
        font = ImageFont.load_default()
        text = f"{label} x {h:g}\n{width} x {height} px" if not whole else f"Enter\n{width} x {height} px"
        d.multiline_text((inset + 3, inset + 3), text, fill=(255, 255, 255, 255), font=font)
    return image


def build(ppu, set_name):
    root = os.path.join(HERE, set_name)
    for sub in ("template", "guides", "blank"):
        os.makedirs(os.path.join(root, sub), exist_ok=True)
    rows = []
    for shape in SHAPES:
        name, label, w, h, whole, used = shape
        width, height = size_px(w, h, ppu)
        guides = draw_guides(shape, ppu)
        blank = Image.new("RGBA", (width, height), (0, 0, 0, 0))
        template = Image.alpha_composite(blank, guides)
        file = f"key_{name}.png"
        template.save(os.path.join(root, "template", file))
        guides.save(os.path.join(root, "guides", file))
        blank.save(os.path.join(root, "blank", file))
        rows.append((file, label, width, height, used, shape))
    return rows


def sheet(rows, ppu, set_name):
    zoom = 6 if ppu == 16 else 1
    gap = 16
    tiles = []
    for file, label, width, height, used, shape in rows:
        guides = Image.open(os.path.join(HERE, set_name, "template", file))
        tiles.append((file, guides.resize((guides.width * zoom, guides.height * zoom), Image.NEAREST), used))
    font = ImageFont.load_default()
    total_w = sum(t[1].width for t in tiles) + gap * (len(tiles) + 1)
    total_h = max(t[1].height for t in tiles) + 70
    canvas = Image.new("RGBA", (total_w, total_h), (40, 40, 48, 255))
    d = ImageDraw.Draw(canvas)
    x = gap
    for file, im, used in tiles:
        canvas.paste(im, (x, gap), im)
        d.text((x, im.height + gap + 6), file.replace(".png", ""), fill=(255, 255, 255, 255), font=font)
        d.text((x, im.height + gap + 20), f"{im.width // zoom} x {im.height // zoom} px", fill=(200, 200, 200, 255), font=font)
        x += im.width + gap
    canvas.save(os.path.join(HERE, f"sheet_{set_name}.png"))


def main():
    table = {}
    for set_name, ppu in SETS.items():
        rows = build(ppu, set_name)
        sheet(rows, ppu, set_name)
        table[set_name] = [(r[0], r[2], r[3], r[4]) for r in rows]
    zip_path = os.path.join(os.path.dirname(HERE), "KeyArt.zip")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as z:
        for base, _, files in os.walk(HERE):
            if "__pycache__" in base:
                continue
            for f in files:
                full = os.path.join(base, f)
                z.write(full, os.path.join("KeyArt", os.path.relpath(full, HERE)))
    for set_name, items in table.items():
        print(set_name)
        for item in items:
            print("  ", item)


main()
