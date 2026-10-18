# 다운로드 캐시의 사운드 .unitypackage 안 오디오 파일 목록을 Docs/SoundCatalog.md 로 정리한다 (AssetIntegrationPlan 4절).
# 사용: python -I Tools/sound_catalog.py   (패키지는 읽기만 한다. 필요한 파일만 extract_audio 로 꺼낸다)
import collections
import glob
import os
import re
import sys
import tarfile

ROOT = os.path.join(os.environ["APPDATA"], "Unity", "Asset Store-5.x")
AUDIO = (".wav", ".ogg", ".mp3", ".aif", ".aiff", ".flac")
KEYS = {
    "audio", "sound", "sfx", "music", "fx", "bgm",
}
CATS = [
    ("BGM", r"music|bgm|track|theme|loop|ambient music|song|saga|melody"),
    ("Ambience", r"ambien|forest|wind|rain|bird|cave|water|night|insect"),
    ("Hit/Impact", r"hit|impact|punch|slash|sword|blade|swing|whoosh|stab"),
    ("Magic", r"magic|spell|fire|ice|frost|lightning|heal|buff|cast"),
    ("UI", r"ui|click|button|menu|select|coin|pickup|notification|confirm|cancel"),
    ("Footstep", r"step|foot|walk|run|jump|land"),
    ("Monster/Voice", r"monster|growl|roar|creature|voice|scream|grunt|death"),
]


def category(path):
    p = path.lower()
    for name, pat in CATS:
        if re.search(pat, p):
            return name
    return "Other"


def scan(pkg):
    files = []
    with tarfile.open(pkg, "r:gz") as t:
        for m in t:
            if m.name.endswith("/pathname"):
                name = t.extractfile(m).read().decode("utf-8", "replace").splitlines()[0]
                if name.lower().endswith(AUDIO):
                    files.append(name)
    return files


def main():
    pkgs = [p for p in glob.glob(os.path.join(ROOT, "**", "*.unitypackage"), recursive=True)
            if any(k in p.lower() for k in KEYS)]
    out = ["# 사운드 카탈로그 (다운로드 캐시, 미임포트)", "",
           "> `Tools/sound_catalog.py`가 생성. 프로젝트에는 쓰는 파일만 `Assets/ThirdParty/Sfx`·`Music`에 넣는다.", ""]
    for pkg in sorted(pkgs):
        size = os.path.getsize(pkg) / 1048576
        try:
            files = scan(pkg)
        except Exception as e:  # 손상·진행 중 다운로드
            out.append("## " + os.path.basename(pkg) + " — 읽기 실패: " + str(e))
            continue
        by = collections.defaultdict(list)
        for f in files:
            by[category(f)].append(f)
        out.append("## %s (%.0f MB, 오디오 %d개)" % (os.path.basename(pkg)[:-13], size, len(files)))
        out.append("")
        for cat in sorted(by, key=lambda c: -len(by[c])):
            items = sorted(by[cat])
            out.append("- **%s** %d: " % (cat, len(items)) + ", ".join(os.path.basename(x) for x in items[:25]) + (" …" if len(items) > 25 else ""))
        out.append("")
        print(os.path.basename(pkg), len(files), flush=True)
    with open(os.path.join("Docs", "SoundCatalog.md"), "w", encoding="utf-8") as fp:
        fp.write("\n".join(out) + "\n")


if __name__ == "__main__":
    sys.exit(main())
