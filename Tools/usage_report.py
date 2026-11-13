#!/usr/bin/env python3
"""세션 transcript 에서 모델별 토큰 사용량 집계 (Opus vs Sonnet 비교용).
사용: python Tools/usage_report.py [--since 2026-11-13T14:00:00Z] [--dir <projects 폴더>]
가중치(공개 단가의 상대 비율, 모델별 절대 단가 아님): input 1, cache_write 1.25, cache_read 0.1, output 5.
주간 한도 %와의 대응은 get_usage 스냅샷과 시각을 맞춰 별도로 본다.
"""
import argparse, glob, json, os, collections

p = argparse.ArgumentParser()
p.add_argument('--since', default='')
p.add_argument('--dir', default=os.path.expanduser('~/.claude/projects/C--Users-user-ProjectB'))
a = p.parse_args()
agg = collections.defaultdict(lambda: collections.Counter())
for f in glob.glob(os.path.join(a.dir, '**', '*.jsonl'), recursive=True):
    try:
        fh = open(f, encoding='utf-8', errors='ignore')
    except OSError:
        continue
    with fh:
        for line in fh:
            try:
                d = json.loads(line)
            except ValueError:
                continue
            ts = d.get('timestamp') or ''
            if a.since and ts < a.since:
                continue
            m = d.get('message')
            if not isinstance(m, dict):
                continue
            u = m.get('usage')
            model = m.get('model')
            if not u or not model or model == '<synthetic>':
                continue
            c = agg[model]
            c['calls'] += 1
            c['in'] += u.get('input_tokens', 0) or 0
            c['out'] += u.get('output_tokens', 0) or 0
            c['cw'] += u.get('cache_creation_input_tokens', 0) or 0
            c['cr'] += u.get('cache_read_input_tokens', 0) or 0
print('model, calls, output_tok, cache_write, cache_read, weighted_units(M)')
for model, c in sorted(agg.items()):
    w = c['in'] + 1.25 * c['cw'] + 0.1 * c['cr'] + 5 * c['out']
    print(f"{model}, {c['calls']}, {c['out']}, {c['cw']}, {c['cr']}, {w/1e6:.2f}")
