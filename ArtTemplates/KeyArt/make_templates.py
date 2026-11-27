import os, zipfile
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
HEIGHT_STRETCH = 2
LEGACY_SHAPES = [
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
BORDER_KEY_UNITS = 3 / 16
BOX_KEY_UNITS = 12 / 16
ENTER = ("enter", "Enter", 1.5, 2.0, True, "L-shaped Enter (ISO/JIS)")
CORNER = (255, 64, 160, 110)
EDGE = (0, 200, 255, 90)
CENTRE = (120, 255, 120, 70)
BODY = (150, 150, 150, 255)
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


def border_px(ppu):
    return round(BORDER_KEY_UNITS * ppu)


def box_px(ppu):
    return round(BOX_KEY_UNITS * ppu)


def box_guides(ppu):
    size, border = box_px(ppu), border_px(ppu)
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(image)
    inner = size - border
    d.rectangle([border, border, inner - 1, inner - 1], fill=CENTRE)
    for box in ([border, 0, inner - 1, border - 1], [border, inner, inner - 1, size - 1], [0, border, border - 1, inner - 1], [inner, border, size - 1, inner - 1]):
        d.rectangle(box, fill=EDGE)
    for box in ([0, 0, border - 1, border - 1], [inner, 0, size - 1, border - 1], [0, inner, border - 1, size - 1], [inner, inner, size - 1, size - 1]):
        d.rectangle(box, fill=CORNER)
    for line in ([border, 0, border, size - 1], [inner, 0, inner, size - 1], [0, border, size - 1, border], [0, inner, size - 1, inner]):
        d.line(line, fill=GUIDE)
    d.rectangle([0, 0, size - 1, size - 1], outline=SAFE)
    return image


def demo_box(ppu):
    size, border = box_px(ppu), border_px(ppu)
    image = Image.new("RGBA", (size, size), (70, 74, 90, 255))
    ImageDraw.Draw(image).rectangle([border, border, size - border - 1, size - border - 1], fill=(205, 208, 220, 255))
    return image


def nine_slice(image, border, width, height):
    size = image.width
    inner = size - border
    out = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    xs_src = [(0, border), (border, inner), (inner, size)]
    ys_src = [(0, border), (border, inner), (inner, size)]
    xs_dst = [(0, border), (border, width - border), (width - border, width)]
    ys_dst = [(0, border), (border, height - border), (height - border, height)]
    for (sy0, sy1), (dy0, dy1) in zip(ys_src, ys_dst):
        for (sx0, sx1), (dx0, dx1) in zip(xs_src, xs_dst):
            part = image.crop((sx0, sy0, sx1, sy1)).resize((dx1 - dx0, dy1 - dy0), Image.NEAREST)
            out.paste(part, (dx0, dy0))
    return out


def build_new(ppu, set_name):
    root = os.path.join(HERE, set_name)
    for sub in ("template", "guides", "blank"):
        os.makedirs(os.path.join(root, sub), exist_ok=True)
    size = box_px(ppu)
    guides = box_guides(ppu)
    blank = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    body = Image.new("RGBA", (size, size), BODY)
    template = Image.alpha_composite(body, guides)
    for sub, image in (("template", template), ("guides", guides), ("blank", blank)):
        image.save(os.path.join(root, sub, "key_box_9slice.png"))
    enter = draw_guides(ENTER, ppu)
    enter_blank = Image.new("RGBA", enter.size, (0, 0, 0, 0))
    enter_template = Image.alpha_composite(enter_blank, enter)
    for sub, image in (("template", enter_template), ("guides", enter), ("blank", enter_blank)):
        image.save(os.path.join(root, sub, "key_enter.png"))
    return [("key_box_9slice.png", size, size), ("key_enter.png", enter.width, enter.height)]


def sheet_new(ppu, set_name):
    zoom = 12 if ppu == 16 else 3
    size, border = box_px(ppu), border_px(ppu)
    template = Image.open(os.path.join(HERE, set_name, "template", "key_box_9slice.png"))
    enter = Image.open(os.path.join(HERE, set_name, "template", "key_enter.png"))
    previews = [(round(w * ppu), round(h * ppu * HEIGHT_STRETCH)) for w, h in ((1, 1), (2.25, 1), (6.25, 1))]
    font = ImageFont.load_default()
    gap = 24
    pieces = [("key_box_9slice", f"{size} x {size} px, border {border} px", template.resize((size * zoom, size * zoom), Image.NEAREST))]
    demo = demo_box(ppu)
    for (w, h) in previews:
        stretched = nine_slice(demo, border, w, h)
        pieces.append((f"{w / ppu:g} key", f"example, {w} x {h} px", stretched.resize((w * max(1, zoom // 3), h * max(1, zoom // 3)), Image.NEAREST)))
    pieces.append(("key_enter", f"{enter.width} x {enter.height} px (not sliced)", enter.resize((enter.width * max(1, zoom // 3), enter.height * max(1, zoom // 3)), Image.NEAREST)))
    total_w = sum(p[2].width for p in pieces) + gap * (len(pieces) + 1)
    total_h = max(p[2].height for p in pieces) + 150
    canvas = Image.new("RGBA", (total_w, total_h), (40, 40, 48, 255))
    d = ImageDraw.Draw(canvas)
    x = gap
    for title, subtitle, im in pieces:
        canvas.paste(im, (x, gap), im)
        d.text((x, im.height + gap + 6), title, fill=(255, 255, 255, 255), font=font)
        d.text((x, im.height + gap + 20), subtitle, fill=(200, 200, 200, 255), font=font)
        x += im.width + gap
    legend = "pink corners never stretch   blue edges stretch one way   green centre stretches both ways"
    d.text((gap, total_h - 50), legend, fill=(255, 255, 255, 255), font=font)
    d.text((gap, total_h - 34), f"Border {border} px = {BORDER_KEY_UNITS:g} of a key width (Key Art: Border Thickness In Key Units).", fill=(200, 200, 200, 255), font=font)
    canvas.save(os.path.join(HERE, f"sheet_{set_name}.png"))


def build(ppu, set_name):
    root = os.path.join(HERE, "legacy_shapes", set_name)
    for sub in ("template", "guides", "blank"):
        os.makedirs(os.path.join(root, sub), exist_ok=True)
    rows = []
    for shape in LEGACY_SHAPES:
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
        guides = Image.open(os.path.join(HERE, "legacy_shapes", set_name, "template", file))
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
    canvas.save(os.path.join(HERE, "legacy_shapes", f"sheet_{set_name}.png"))


def main():
    table = {}
    for set_name, ppu in SETS.items():
        table[set_name] = build_new(ppu, set_name)
        sheet_new(ppu, set_name)
        rows = build(ppu, set_name)
        sheet(rows, ppu, set_name)
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
