"""어비스 안개 띠 (ArtStyle.md 머리말: 안개 낀 깊이). 24 PPU 도트, 가로로 이어 붙는 텍스처.
사용: python Tools/pixelfx/gen_fog.py  → Assets/Resources/Atmosphere/FogFar.png, FogNear.png
- 값 노이즈(두 옥타브)를 가로 주기에 맞춰 이음매 없이 만들고, 위아래로 갈수록 옅게 한 뒤 알파를 4단으로 끊는다.
- 색은 옅은 청록(먼 안개는 더 푸르고 밝게). 조명은 Unity Sprite-Lit 재질이 낮밤을 따라간다."""
import os
import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "Atmosphere")


def tile_noise(w, h, cells_x, cells_y, rng):
    grid = rng.random((cells_y + 1, cells_x))
    grid = np.concatenate([grid, grid[:, :1]], axis=1)  # 가로 이음매
    ys = np.linspace(0, cells_y, h, endpoint=False)
    xs = np.linspace(0, cells_x, w, endpoint=False)
    y0, x0 = ys.astype(int), xs.astype(int)
    fy, fx = ys - y0, xs - x0
    fy, fx = fy * fy * (3 - 2 * fy), fx * fx * (3 - 2 * fx)
    a = grid[y0][:, x0]; b = grid[y0][:, x0 + 1]
    c = grid[y0 + 1][:, x0]; d = grid[y0 + 1][:, x0 + 1]
    top = a + (b - a) * fx[None]; bot = c + (d - c) * fx[None]
    return top + (bot - top) * fy[:, None]


def band(w, h, color, max_alpha, seed):
    rng = np.random.default_rng(seed)
    n = 0.65 * tile_noise(w, h, 8, 3, rng) + 0.35 * tile_noise(w, h, 24, 6, rng)
    y = np.linspace(-1, 1, h)[:, None]
    falloff = np.clip(1 - np.abs(y) ** 1.6, 0, 1)
    v = np.clip((n - 0.25) / 0.6, 0, 1) * falloff
    steps = np.array([0.0, 0.35, 0.6, 0.85, 1.0])
    q = steps[np.clip((v * 4.5).astype(int), 0, 4)]
    # 4×4 베이어 디더로 단계 경계를 도트답게
    bayer = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0
    dither = np.tile(bayer, (h // 4 + 1, w // 4 + 1))[:h, :w]
    q = np.where((v * 4.5) % 1 > dither + 0.25, steps[np.clip((v * 4.5).astype(int) + 1, 0, 4)], q)
    img = np.zeros((h, w, 4), np.uint8)
    img[..., 0], img[..., 1], img[..., 2] = color
    img[..., 3] = (q * max_alpha).astype(np.uint8)
    return Image.fromarray(img, "RGBA")


def main():
    os.makedirs(OUT, exist_ok=True)
    band(480, 96, (176, 214, 216), 150, 7).save(os.path.join(OUT, "FogFar.png"))
    band(480, 72, (150, 196, 196), 120, 11).save(os.path.join(OUT, "FogNear.png"))
    print("fog ok")


if __name__ == "__main__":
    main()
