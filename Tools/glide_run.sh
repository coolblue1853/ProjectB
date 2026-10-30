#!/usr/bin/env bash
# 활공 드라이버 한 번 실행: bash Tools/glide_run.sh <mode> <대기초>
cd "$(dirname "$0")/.."
sed "s/__MODE__/$1/" Tools/probes/glide_drive.cs > Tools/probes/_glide_drive_run.cs
bash Tools/u.sh cs Tools/probes/_glide_drive_run.cs
sleep "${2:-10}"
bash Tools/u.sh cs Tools/probes/glide_read.cs
rm -f Tools/probes/_glide_drive_run.cs
