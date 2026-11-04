import re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
p = r'E:\glsa\ScoreCardUnity\Assets\Mirza\Solaris\Shaders\Solaris.shader'
s = open(p, encoding='utf-8').read()
lines = s.split('\n')
top = {}
for ln in lines[:230]:
    for m in re.finditer(r'(_[A-Za-z0-9_]+)\(\s*"([^"]+)"', ln):
        top[m.group(1)] = m.group(2)
graph = {}
for ln in lines[5795:]:
    for m in re.finditer(r'"Property","(_[A-Za-z0-9_]+)","([^"]+)"', ln):
        graph[m.group(1)] = m.group(2)
print('top count:', len(top), 'graph count:', len(graph))
print('only in top:', set(top) - set(graph))
print('only in graph:', set(graph) - set(top))
mismatch = [(k, top[k], graph[k]) for k in top if k in graph and top[k] != graph[k]]
print('mismatch count:', len(mismatch))
for k, a, b in mismatch:
    print(k, '| top:', a, '| graph:', b)
en = [(k, v) for k, v in graph.items() if re.match(r'[A-Za-z]', v)]
print('english-leading graph names:', en)
