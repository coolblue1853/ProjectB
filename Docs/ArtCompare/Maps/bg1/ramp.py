import sys
from PIL import Image,ImageEnhance,ImageChops,ImageDraw
def grade(im,bright,sat,tint,amt):
    a=im.split()[3];rgb=im.convert('RGB')
    rgb=ImageEnhance.Brightness(rgb).enhance(bright);rgb=ImageEnhance.Color(rgb).enhance(sat)
    t=Image.new('RGB',rgb.size,tint);rgb=Image.blend(rgb,t,amt);o=rgb.convert('RGBA');o.putalpha(a);return o
def fade_alpha(im,k):
    a=im.split()[3].point(lambda v:int(v*k));o=im.copy();o.putalpha(a);return o
def fog(size,col,alpha):
    f=Image.new('RGBA',size,col+(0,));d=ImageDraw.Draw(f)
    for y in range(size[1]):
        d.line((0,y,size[0],y),fill=col+(int(alpha*(y/size[1])),))
    return f
def scene(far,mid,near,hero,mob,fogcol,fogalpha,ground):
    W=Image.new('RGBA',(512,128),(0,0,0,255))
    for x in (0,256): W.alpha_composite(far,(x,0))
    for x in (0,256): W.alpha_composite(mid,(x,0))
    W.alpha_composite(fog((512,128),fogcol,fogalpha))
    d=ImageDraw.Draw(W);d.rectangle((0,106,511,127),fill=ground);d.rectangle((0,106,511,107),fill=tuple(min(255,c+40) for c in ground))
    hb=hero.crop(hero.getbbox());W.alpha_composite(hb,(110,107-hb.height))
    gb=mob.crop(mob.getbbox());W.alpha_composite(gb,(300,107-gb.height))
    n=fade_alpha(near,0.85)
    sh=int(near.height*0.36)   # 아래로 내려 풀끝만 바닥선 위로 보이게(자르지 않고 이동)
    for x in (0,256): W.alpha_composite(n,(x,sh))
    return W
