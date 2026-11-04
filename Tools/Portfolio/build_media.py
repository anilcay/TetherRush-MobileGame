"""Package real Unity captures for GitHub. Python 3 + Pillow; no external media binaries.

Exports a vector brand banner and animated preview, moves an MP4's index ahead
of its media data for progressive playback, and verifies the container metadata.
Run only after the explicit PortfolioCapture/Gallery Unity tests.
"""
from pathlib import Path
import hashlib
import json
import struct
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
MEDIA = ROOT / 'Docs/Portfolio/Media'
OUT = ROOT / 'output/portfolio'


def boxes(data, start=0, end=None):
    end = len(data) if end is None else end
    while start < end:
        assert start + 8 <= end, 'Truncated MP4 header'
        size, kind = struct.unpack_from('>I4s', data, start)
        header = 8
        if size == 1:
            size, = struct.unpack_from('>Q', data, start + 8)
            header = 16
        elif size == 0:
            size = end - start
        assert size >= header and start + size <= end, (kind, size)
        yield kind.decode('ascii'), start, start + size, header
        start += size


def children(data, box):
    return list(boxes(data, box[1] + box[3], box[2]))


def find(data, box, name):
    return next(b for b in children(data, box) if b[0] == name)


def fast_start(path):
    data = path.read_bytes()
    top = list(boxes(data))
    moov = next(b for b in top if b[0] == 'moov')
    mdat = next(b for b in top if b[0] == 'mdat')
    if moov[1] < mdat[1]:
        return False
    ftyp = next(b for b in top if b[0] == 'ftyp')
    insert = ftyp[2]
    atom = bytearray(data[moov[1]:moov[2]])
    containers = {'moov', 'trak', 'mdia', 'minf', 'stbl', 'edts', 'dinf', 'udta'}

    def patch(start, end):
        for kind, a, b, header in boxes(atom, start, end):
            if kind in ('stco', 'co64'):
                count, = struct.unpack_from('>I', atom, a + header + 4)
                fmt, width = ('>I', 4) if kind == 'stco' else ('>Q', 8)
                for i in range(count):
                    pos = a + header + 8 + i * width
                    offset, = struct.unpack_from(fmt, atom, pos)
                    if insert <= offset < moov[1]:
                        offset += len(atom)
                    struct.pack_into(fmt, atom, pos, offset)
            elif kind in containers:
                patch(a + header, b)
    patch(0, len(atom))
    result = data[:insert] + atom + data[insert:moov[1]] + data[moov[2]:]
    assert len(result) == len(data)
    path.write_bytes(result)
    return True


def inspect_mp4(path):
    data = path.read_bytes()
    top = list(boxes(data))
    moov = next(b for b in top if b[0] == 'moov')
    mdat = next(b for b in top if b[0] == 'mdat')
    tracks = []
    for trak in (b for b in children(data, moov) if b[0] == 'trak'):
        mdia = find(data, trak, 'mdia')
        mdhd = find(data, mdia, 'mdhd'); pos = mdhd[1] + mdhd[3]
        version = data[pos]
        if version == 0:
            scale, duration = struct.unpack_from('>II', data, pos + 12)
        else:
            scale, = struct.unpack_from('>I', data, pos + 20)
            duration, = struct.unpack_from('>Q', data, pos + 24)
        hdlr = find(data, mdia, 'hdlr')
        handler = data[hdlr[1] + hdlr[3] + 8:hdlr[1] + hdlr[3] + 12].decode('ascii')
        stbl = find(data, find(data, mdia, 'minf'), 'stbl')
        stsd = find(data, stbl, 'stsd')
        entry = next(boxes(data, stsd[1] + stsd[3] + 8, stsd[2]))
        stsz = find(data, stbl, 'stsz')
        count, = struct.unpack_from('>I', data, stsz[1] + stsz[3] + 8)
        track = {'type': handler, 'codec': entry[0], 'duration_s': duration / scale, 'sample_count': count}
        if handler == 'vide':
            track['width'], track['height'] = struct.unpack_from('>HH', data, entry[1] + entry[3] + 24)
        for chunk in (b for b in children(data, stbl) if b[0] in ('stco', 'co64')):
            n, = struct.unpack_from('>I', data, chunk[1] + chunk[3] + 4)
            fmt, step = ('>I', 4) if chunk[0] == 'stco' else ('>Q', 8)
            for i in range(n):
                offset, = struct.unpack_from(fmt, data, chunk[1] + chunk[3] + 8 + i * step)
                assert mdat[1] + mdat[3] <= offset < mdat[2], 'Invalid chunk offset after fast-start packaging'
        tracks.append(track)
    video = next(t for t in tracks if t['type'] == 'vide')
    audio = next(t for t in tracks if t['type'] == 'soun')
    assert video['codec'] == 'avc1' and audio['codec'] == 'mp4a'
    assert (video['width'], video['height'], video['sample_count']) == (540, 960, 450)
    assert abs(video['duration_s'] - 15) < .05 and abs(audio['duration_s'] - 15) < .15
    return {'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest(),
            'index_precedes_media': moov[1] < mdat[1], 'tracks': tracks}


def banner():
    # Code-native vector branding; actual game imagery is shown separately below it.
    svg = '''<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="360" viewBox="0 0 1200 360" role="img" aria-labelledby="title desc">
<title id="title">Tether Rush</title><desc id="desc">Two cores. One connection. A portrait arcade runner.</desc>
<defs><radialGradient id="peach" cx="30%" cy="25%"><stop stop-color="#ffe7d5"/><stop offset="1" stop-color="#ffb48f"/></radialGradient><radialGradient id="purple" cx="30%" cy="25%"><stop stop-color="#e2dcff"/><stop offset="1" stop-color="#b5a4ff"/></radialGradient></defs>
<rect width="1200" height="360" rx="0" fill="#0d1722"/>
<path d="M0 359H1200" stroke="#314356"/>
<g font-family="Arial,Helvetica,sans-serif"><text x="64" y="76" fill="#8293a3" font-size="15" letter-spacing="4">ONE-TOUCH ARCADE</text>
<text x="60" y="170" fill="#f5f2e9" font-size="76" font-weight="900" letter-spacing="-3">TETHER RUSH</text>
<text x="64" y="225" fill="#f5f2e9" font-size="25">Two cores. One connection.</text>
<text x="64" y="292" fill="#8293a3" font-size="17">25 LEVELS  /  ENDLESS FLOW  /  ANDROID</text></g>
<path d="M850 180H1080" stroke="#42dff5" stroke-width="5" stroke-linecap="round"/>
<circle cx="850" cy="180" r="51" fill="url(#peach)"/><circle cx="1080" cy="180" r="51" fill="url(#purple)"/>
<circle cx="850" cy="180" r="51" fill="none" stroke="#ffdac7" stroke-opacity=".4"/>
<circle cx="1080" cy="180" r="51" fill="none" stroke="#ded6ff" stroke-opacity=".4"/>
<circle cx="835" cy="151" r="5" fill="#30465a"/><circle cx="1065" cy="151" r="5" fill="#30465a"/>
</svg>'''
    (MEDIA / 'cover.svg').write_text(svg, encoding='utf-8')


def gif_preview():
    files = sorted((OUT / 'gif-frames').glob('frame*.png'))
    assert len(files) == 150, 'Expected 15 seconds at 10 fps'
    frames = []
    for file in files:
        with Image.open(file) as source:
            image = source.convert('RGB').resize((216, 384), Image.Resampling.LANCZOS)
            frames.append(image.quantize(colors=96, method=Image.Quantize.MEDIANCUT))
    path = MEDIA / 'trailer-preview.gif'
    frames[0].save(path, save_all=True, append_images=frames[1:], duration=100, loop=0,
                   optimize=True, disposal=2)
    with Image.open(path) as result:
        duration = 0
        for i in range(result.n_frames):
            result.seek(i); duration += result.info['duration']
        assert result.n_frames == 150 and duration == 15000
    return {'width': 216, 'height': 384, 'frames': 150, 'duration_s': 15,
            'bytes': path.stat().st_size, 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}


if __name__ == '__main__':
    MEDIA.mkdir(parents=True, exist_ok=True)
    banner()
    moved = fast_start(MEDIA / 'trailer.mp4')
    report = {'video': inspect_mp4(MEDIA / 'trailer.mp4'), 'index_relocated': moved,
              'gif': gif_preview(), 'capture': json.loads((OUT / 'movie.json').read_text())}
    report['source_note'] = 'Real Unity Editor gameplay with scripted SafeRoute input; original music + event-timed editorial SFX; not phone/DSP footage.'
    (ROOT / 'Docs/Portfolio/media-validation.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k not in ('capture',)}, indent=2))
