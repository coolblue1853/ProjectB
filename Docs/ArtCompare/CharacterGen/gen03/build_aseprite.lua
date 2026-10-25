-- gen03 기본 체형 Aseprite 작업 파일 생성(배치 실행용)
local d = app.params["dir"]
local spr = Sprite(64, 64, ColorMode.RGB)
local function addLayer(name, png, opacity, editable)
  local layer = spr:newLayer()
  layer.name = name
  local img = Image{ fromFile = d .. "/" .. png }
  spr:newCel(layer, 1, img, Point(0, 0))
  layer.opacity = opacity
  layer.isEditable = editable
  return layer
end
addLayer("원본_참조(숨김)", "../../hero_style_anchor_64.png", 255, false).isVisible = false
addLayer("원본_고정(얼굴눈입다리)", "layer_keep.png", 255, false)
addLayer("다듬을곳(머리몸팔)", "layer_edit.png", 255, true)
spr:deleteLayer(spr.layers[1])
local pal = Palette(0)
for line in io.lines(d .. "/palette.txt") do
  local r, g, b = line:match("(%d+) (%d+) (%d+)")
  pal:resize(#pal + 1)
  pal:setColor(#pal - 1, Color{ r = tonumber(r), g = tonumber(g), b = tonumber(b) })
end
spr:setPalette(pal)
spr:saveAs(d .. "/base_body_v3_edit.aseprite")
