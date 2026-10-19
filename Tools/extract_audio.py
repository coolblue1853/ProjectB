# 다운로드 캐시의 .unitypackage 에서 오디오 파일 몇 개만 꺼내 OGG 로 변환해 프로젝트에 넣는다(패키지 전체 임포트 없이).
# 사용: python -I Tools/extract_audio.py "<패키지 이름 일부>" "<출력 폴더>" "파일1.wav" "파일2.wav" ...
import glob
import os
import subprocess
import sys
import tarfile
import tempfile

ROOT = os.path.join(os.environ["APPDATA"], "Unity", "Asset Store-5.x")


def main():
    pkg_key, out_dir, wanted = sys.argv[1], sys.argv[2], set(sys.argv[3:])
    pkgs = [p for p in glob.glob(os.path.join(ROOT, "**", "*.unitypackage"), recursive=True) if pkg_key in os.path.basename(p)]
    if not pkgs:
        print("package not found:", pkg_key)
        return 1
    os.makedirs(out_dir, exist_ok=True)
    names = {}
    with tarfile.open(pkgs[0], "r:gz") as t:
        for m in t:
            if m.name.endswith("/pathname"):
                path = t.extractfile(m).read().decode("utf-8", "replace").splitlines()[0]
                if os.path.basename(path) in wanted:
                    names[m.name.split("/")[0]] = os.path.basename(path)
    with tarfile.open(pkgs[0], "r:gz") as t, tempfile.TemporaryDirectory() as tmp:
        for m in t:
            guid = m.name.split("/")[0]
            if m.name.endswith("/asset") and guid in names:
                src = os.path.join(tmp, names[guid])
                with open(src, "wb") as fp:
                    fp.write(t.extractfile(m).read())
                dst = os.path.join(out_dir, os.path.splitext(names[guid])[0] + ".ogg")
                subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", src, "-vn", "-c:a", "libvorbis", "-q:a", "5", dst], check=True)
                print("ok", dst, os.path.getsize(dst) // 1024, "KB")
                wanted.discard(names[guid])
    for w in wanted:
        print("missing", w)
    return 0


if __name__ == "__main__":
    sys.exit(main())
