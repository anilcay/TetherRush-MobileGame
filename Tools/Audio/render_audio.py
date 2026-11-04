"""Original deterministic Tether Rush score + sound design. Python 3 + numpy.

Uses Unity's installed Xiph DLLs only for Vorbis encoding/decoding. No samples.
Run from anywhere; --codec-dir can override Unity's FSBTool/x64 directory.
"""
from pathlib import Path
import argparse, ctypes as C, json, math, wave
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'AudioSource'
ASSETS = ROOT / 'Assets/TetherRush/Resources/Audio'
SR = 48000
RNG = np.random.default_rng(1042026)

def hz(midi): return 440 * 2 ** ((midi - 69) / 12)
def timeline(seconds, sr=SR): return np.arange(round(seconds * sr), dtype=np.float64) / sr
def soften(x, sr=SR, attack=.005, release=.025):
    x = np.array(x, dtype=np.float64, copy=True)
    a, r = min(len(x), round(attack*sr)), min(len(x), round(release*sr))
    fade_in = np.sin(np.linspace(0, np.pi/2, a)) ** 2
    fade_out = np.sin(np.linspace(np.pi/2, 0, r)) ** 2
    x[:a] *= fade_in if x.ndim == 1 else fade_in[:, None]
    x[-r:] *= fade_out if x.ndim == 1 else fade_out[:, None]
    return x
def tone(note, dur, kind='pluck', sr=SR):
    t = timeline(dur, sr); f = hz(note)
    if kind == 'pad':
        x = sum(np.sin(2*np.pi*f*k*t + p) * a for k,a,p in [(1,1,0),(2,.18,.4),(3,.055,.8)])
        x = x * (.97 + .03*np.sin(2*np.pi*.21*t))
        return soften(x, sr, .35, .65) * .38
    if kind == 'bass':
        x = np.sin(2*np.pi*f*t) + .19*np.sin(2*np.pi*f*2*t) + .035*np.sin(2*np.pi*f*3*t)
        return soften(x*np.exp(-t/1.0), sr, .014, .10)
    # Rounded mallet: inharmonic partials decay independently, quiet transient.
    x = sum(a*np.sin(2*np.pi*f*k*t)*np.exp(-t/tau) for k,a,tau in [(1,1,.46),(2.008,.19,.19),(3.97,.07,.08),(5.2,.022,.035)])
    return soften(x, sr, .004, .08)
def kick():
    t=timeline(.38); phase=2*np.pi*(48*t+38*.033*(1-np.exp(-t/.033)))
    return soften((np.sin(phase)*np.exp(-t/.095)+RNG.standard_normal(len(t))*.025*np.exp(-t/.006)), attack=.002)
def noise(dur, decay, amp=1):
    t=timeline(dur); n=RNG.standard_normal(len(t))
    # Differenced noise gives shakers a dry airy spectrum; no sharp click.
    return soften(np.diff(n,prepend=n[0])*np.exp(-t/decay), attack=.003, release=.015)*amp
def stereo(x, pan):
    return np.stack((x*np.sqrt((1-pan)/2),x*np.sqrt((1+pan)/2)),axis=1)
def add_wrap(dst, x, start, gain=1, pan=0):
    x=stereo(x,pan) if x.ndim==1 else x
    pos=round(start*SR)%len(dst); n=len(x)
    if pos+n <= len(dst): dst[pos:pos+n] += x*gain
    else:
        count=len(dst)-pos; dst[pos:] += x[:count]*gain; dst[:n-count] += x[count:]*gain
def k_lufs(x):
    # ITU-R BS.1770 K weighting, exact 48k coefficients, periodic FFT filtering.
    w=2*np.pi*np.fft.rfftfreq(len(x)); z=np.exp(-1j*w)
    h=(1.53512485958697-2.69169618940638*z+1.19839281085285*z*z)/(1-1.69065929318241*z+.73248077421585*z*z)
    h *= (1-2*z+z*z)/(1-1.99004745483398*z+.99007225036621*z*z)
    y=np.fft.irfft(np.fft.rfft(x,axis=0)*h[:,None],n=len(x),axis=0)
    power=np.sum(y*y,axis=1); cumulative=np.concatenate(([0],np.cumsum(power)))
    width=round(.4*SR); starts=np.arange(0,len(x)-width+1,round(.1*SR))
    e=(cumulative[starts+width]-cumulative[starts])/width
    e=e[e>10**((-70+.691)/10)]
    threshold=np.mean(e)*.1; e=e[e>threshold]
    return float(-.691+10*np.log10(np.mean(e)))
def stats(x, sr):
    peak=float(np.max(np.abs(x))); rms=float(np.sqrt(np.mean(x*x)))
    return dict(duration_s=round(len(x)/sr,6),sample_rate=sr,channels=1 if x.ndim==1 else x.shape[1],peak_dbfs=round(20*np.log10(max(peak,1e-12)),3),rms_dbfs=round(20*np.log10(max(rms,1e-12)),3),dc_mean=float(np.mean(x)),finite=bool(np.isfinite(x).all()),clipped_samples=int(np.sum(np.abs(x)>=1)))
def wav(path,x,sr):
    path.parent.mkdir(parents=True,exist_ok=True)
    with wave.open(str(path),'wb') as out:
        out.setnchannels(1 if x.ndim==1 else x.shape[1]); out.setsampwidth(2); out.setframerate(sr)
        out.writeframes(np.round(np.clip(x,-1,1)*32767).astype('<i2').tobytes())

class Packet(C.Structure):
    _fields_=[('packet',C.POINTER(C.c_ubyte)),('bytes',C.c_long),('b_o_s',C.c_long),('e_o_s',C.c_long),('granulepos',C.c_longlong),('packetno',C.c_longlong)]
class Page(C.Structure):
    _fields_=[('header',C.c_void_p),('header_len',C.c_long),('body',C.c_void_p),('body_len',C.c_long)]
def opaque(): return C.create_string_buffer(4096)
def encode_ogg(path,x,codec):
    directory_handle=__import__('os').add_dll_directory(str(codec))
    ogg=C.CDLL(str(codec/'libogg64.dll')); v=C.CDLL(str(codec/'libvorbis64.dll'))
    v.vorbis_encode_init_vbr.argtypes=[C.c_void_p,C.c_long,C.c_long,C.c_float]
    v.vorbis_analysis_buffer.argtypes=[C.c_void_p,C.c_int]; v.vorbis_analysis_buffer.restype=C.POINTER(C.POINTER(C.c_float))
    info,comment,dsp,block,stream=[opaque() for _ in range(5)]
    v.vorbis_info_init(info); assert v.vorbis_encode_init_vbr(info,2,SR,.4)==0
    v.vorbis_comment_init(comment)
    v.vorbis_comment_add_tag(comment,b'TITLE',b'Tether Flow'); v.vorbis_comment_add_tag(comment,b'ARTIST',b'Tether Rush original synthesis')
    assert v.vorbis_analysis_init(dsp,info)==0; assert v.vorbis_block_init(dsp,block)==0
    assert ogg.ogg_stream_init(stream,1042026)==0
    p1,p2,p3=Packet(),Packet(),Packet(); pg=Page()
    v.vorbis_analysis_headerout(dsp,comment,C.byref(p1),C.byref(p2),C.byref(p3))
    with open(path,'wb') as out:
        def pages(flush=False):
            fn=ogg.ogg_stream_flush if flush else ogg.ogg_stream_pageout
            while fn(stream,C.byref(pg)):
                out.write(C.string_at(pg.header,pg.header_len)); out.write(C.string_at(pg.body,pg.body_len))
        for p in [p1,p2,p3]: ogg.ogg_stream_packetin(stream,C.byref(p))
        pages(True)
        for offset in range(0,len(x)+1024,1024):
            chunk=x[offset:offset+1024]; count=len(chunk)
            if count:
                buffers=v.vorbis_analysis_buffer(dsp,count)
                for c in range(2):
                    data=np.ascontiguousarray(chunk[:,c],dtype=np.float32); C.memmove(buffers[c],data.ctypes.data,count*4)
            v.vorbis_analysis_wrote(dsp,count)
            while v.vorbis_analysis_blockout(dsp,block)==1:
                v.vorbis_analysis(block,None); v.vorbis_bitrate_addblock(block)
                packet=Packet()
                while v.vorbis_bitrate_flushpacket(dsp,C.byref(packet)):
                    ogg.ogg_stream_packetin(stream,C.byref(packet)); pages()
            if not count: break
        pages(True)
    ogg.ogg_stream_clear(stream); v.vorbis_block_clear(block); v.vorbis_dsp_clear(dsp); v.vorbis_comment_clear(comment); v.vorbis_info_clear(info)
    directory_handle.close()
def decode_ogg(path,codec):
    handle=__import__('os').add_dll_directory(str(codec)); v=C.CDLL(str(codec/'libvorbisfile64.dll'))
    vf=opaque(); v.ov_fopen.argtypes=[C.c_char_p,C.c_void_p]
    # Windows CRT fopen does not accept UTF-8 project paths. Only pass the
    # ASCII filename after selecting its directory with Python's Unicode API.
    import os
    previous=Path.cwd()
    try:
        os.chdir(path.parent)
        result=v.ov_fopen(path.name.encode('ascii'),vf)
    finally:
        os.chdir(previous)
    assert result==0
    v.ov_read_float.argtypes=[C.c_void_p,C.POINTER(C.POINTER(C.POINTER(C.c_float))),C.c_int,C.POINTER(C.c_int)]
    chunks=[]; bitstream=C.c_int(); buf=C.POINTER(C.POINTER(C.c_float))()
    while True:
        count=v.ov_read_float(vf,C.byref(buf),4096,C.byref(bitstream))
        if count==0: break
        assert count>0
        chunks.append(np.stack([np.ctypeslib.as_array(buf[c],shape=(count,)).copy() for c in range(2)],axis=1))
    v.ov_clear(vf); handle.close(); return np.concatenate(chunks)

def render_music(score):
    beat=60/score['bpm']; bar=beat*4; n=round(32*bar*SR)
    dry=np.zeros((n,2)); wet=np.zeros((n,2)); rhythm=np.zeros((n,2))
    chords=score['chords']; melody=score['melody_midi']
    for b in range(32):
        section=b//8; chord=chords[(b//2)%4]; root=chord['bass']
        for i,note in enumerate(chord['voicing']):
            add_wrap(wet,tone(note,bar*2+.65,'pad'),b*bar,.027,(-.65+i*.32)) if b%2==0 else None
        for step,length,velocity in [(0,1.35,.115),(1.75,.65,.073),(2.5,1.15,.095)]:
            add_wrap(dry,tone(root,length*beat,'bass'),b*bar+step*beat,velocity)
        for step,amp in [(0,.14),(2,.115)]: add_wrap(rhythm,kick(),b*bar+step*beat,amp)
        # Soft rim and hand shaker, subtly displaced second/last eighths.
        for step in [1,3]:
            t=timeline(.13); rim=soften((np.sin(2*np.pi*410*t)*.26+noise(.13,.018,.17))*np.exp(-t/.022),attack=.0015)
            add_wrap(rhythm,rim,b*bar+step*beat,.10,.18)
        for eighth in range(8):
            if section==0 and b%8<2 and eighth%2==0: continue
            add_wrap(rhythm,noise(.075,.018),b*bar+(eighth*.5+(0.035 if eighth%2 else 0))*beat,.006 if eighth%2==0 else .009,(-.40 if eighth%2==0 else .40))
        # Eight-bar call-and-response motif; final beat left open for the loop.
        events=melody[b%8]
        for step,note in events:
            octave=0 if section!=2 else (12 if step==2.75 else 0)
            add_wrap(wet,tone(note+octave,1.5),b*bar+step*beat,.068 if section<2 else .061,(-.20 if b%2==0 else .22))
        # Small answering chord on alternate bars; never constant arpeggiation.
        if b%2==1:
            for i,note in enumerate(chord['voicing'][1:3]): add_wrap(wet,tone(note+12,.85),b*bar+(2.5+i*.25)*beat,.019,(-.35+i*.7))
    # Periodic delay + diffuse cross-channel late reflections. np.roll wraps ALL
    # tails into the beginning: this is a continuous cyclic mix, no intro fade.
    mix=dry+rhythm+wet
    for delay,gain in [(beat*.75,.23),(beat*1.5,.105),(beat*2.25,.045)]:
        mix+=np.roll(wet[:,::-1],round(delay*SR),axis=0)*gain
    for delay,gain in [(.071,.12),(.113,.11),(.181,.10),(.263,.075),(.397,.05),(.557,.035),(.773,.022)]:
        mix+=np.roll(wet[:,::-1],round(delay*SR),axis=0)*gain
    mix-=mix.mean(axis=0)
    mix *= 10**((-20-k_lufs(mix))/20)
    assert np.max(np.abs(mix))<.91
    return mix

def render_sfx():
    global RNG
    RNG=np.random.default_rng(4402026)
    sr=44100; cues={}
    def bell(note,dur=.34): return tone(note,dur,sr=sr)
    def stack(events,dur):
        x=np.zeros(round(dur*sr))
        for start,note,gain in events:
            p=round(start*sr); a=bell(note,min(.43,dur-start)); x[p:p+len(a)]+=a*gain
        return x
    for i,notes in enumerate([(86,93),(89,96),(93,98)],1):
        cues[f'Coin{i}']=stack([(0,notes[0],.8),(.047,notes[1],.55)],.39)
    t=timeline(.24,sr); f=430+260*(1-np.exp(-t/.025))
    phase=2*np.pi*np.cumsum(f)/sr
    cues['Energy']=soften((np.sin(phase+.55*np.sin(phase*2))*.8+np.sin(phase*.5)*.18)*np.exp(-t/.065),sr,.006,.04)
    cues['Ready']=stack([(0,74,.5),(.075,81,.5),(.15,86,.62)],.57)
    t=timeline(.49,sr); phase=2*np.pi*(47*t+91*.032*(1-np.exp(-t/.032)))
    thump=np.sin(phase)*np.exp(-t/.09)
    shimmer=(np.sin(2*np.pi*1174*t)+.26*np.sin(2*np.pi*1762*t))*np.exp(-t/.105)*.13
    cues['Pulse']=soften(thump+shimmer,sr,.003,.07)
    cues['Relay']=stack([(0,62,.65),(.07,69,.55),(.13,74,.45)],.46)
    cues['Fail']=stack([(0,62,.65),(.085,57,.6),(.17,50,.7)],.59)
    cues['Success']=stack([(0,74,.65),(.12,77,.58),(.25,81,.65),(.25,86,.26)],.91)
    t=timeline(.085,sr); click=(np.sin(2*np.pi*680*t)*np.exp(-t/.009)+.10*RNG.standard_normal(len(t))*np.exp(-t/.003))
    cues['UiClick']=soften(click,sr,.001,.018)
    levels={'Energy':-.0,'Coin1':0,'Coin2':0,'Coin3':0,'Ready':-1.5,'Pulse':0,'Relay':-1,'Fail':-1,'Success':-1,'UiClick':-5}
    for name,x in cues.items():
        x-=x.mean(); x=soften(x,sr,.001,.015)
        correction_window=np.sin(np.linspace(0,np.pi,len(x)))**2
        x-=x.sum()/correction_window.sum()*correction_window
        x*=10**((-6+levels[name])/20)/np.max(np.abs(x))
        cues[name]=np.concatenate((np.zeros(round(.002*sr)),x,np.zeros(round(.012*sr))))
    return cues

def render_preview(music,cues):
    """Actual default runtime gains, no post-mix normalization."""
    preview=music[:14*SR].copy()*.50*.55
    for seconds,name in [(0.25,'UiClick'),(1.4,'Energy'),(2.2,'Coin1'),(2.6,'Coin2'),(3.0,'Coin3'),(4.5,'Energy'),(5.2,'Ready'),(6.0,'Pulse'),(7.8,'Relay'),(10.2,'Success')]:
        gain=(.9 if name.startswith('Coin') else .8)*.55
        x=cues[name]; resampled=np.interp(np.arange(round(len(x)*SR/44100))*44100/SR,np.arange(len(x)),x)
        start=round(seconds*SR); preview[start:start+len(resampled)]+=stereo(resampled,0)*gain
    preview=soften(preview,SR,.025,.35)
    wav(SOURCE/'TetherFlow.gameplay-preview.wav',preview,SR)
    return preview

def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--codec-dir',type=Path,default=Path('C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Data/Tools/FSBTool/x64')); parser.add_argument('--reuse-master',action='store_true'); args=parser.parse_args()
    SOURCE.mkdir(exist_ok=True); ASSETS.mkdir(parents=True,exist_ok=True)
    score=json.loads((SOURCE/'TetherFlow.score.json').read_text())
    if args.reuse_master:
        with wave.open(str(SOURCE/'TetherFlow.master.wav'),'rb') as source:
            music=np.frombuffer(source.readframes(source.getnframes()),dtype='<i2').reshape(-1,2).astype(np.float64)/32767
    else:
        music=render_music(score); wav(SOURCE/'TetherFlow.master.wav',music,SR)
        # Codec input is the checked-in PCM master, so --reuse-master and a
        # complete render export exactly the same file, including quantization.
        music=np.round(music*32767).astype('<i2').astype(np.float64)/32767
    encode_ogg(ASSETS/'TetherFlow.ogg',music,args.codec_dir)
    decoded=decode_ogg(ASSETS/'TetherFlow.ogg',args.codec_dir)
    report={'music_master':stats(music,SR),'music_vorbis_decoded':stats(decoded,SR),'sfx':{},'human_audition':'Not performed by renderer. Listening and Unity playback QA remain separate.'}
    report['music_master']['integrated_lufs']=round(k_lufs(music),3)
    report['music_master']['loop_seam_step']=float(np.max(np.abs(music[0]-music[-1])))
    report['music_vorbis_decoded']['integrated_lufs']=round(k_lufs(decoded),3)
    report['music_vorbis_decoded']['loop_seam_step']=float(np.max(np.abs(decoded[0]-decoded[-1])))
    report['music_vorbis_decoded']['frame_count_matches']=len(decoded)==len(music)
    report['music_vorbis_decoded']['bytes']=(ASSETS/'TetherFlow.ogg').stat().st_size
    cues=render_sfx()
    for name,x in cues.items():
        wav(ASSETS/f'{name}.wav',x,44100)
        pcm=np.round(x*32767).astype('<i2').astype(np.float64)/32767
        report['sfx'][name]=stats(pcm,44100)
        report['sfx'][name]['silent_first_last_50_samples']=bool(np.max(np.abs(pcm[:50]))==0 and np.max(np.abs(pcm[-50:]))==0)
        assert np.max(np.abs(x[:50]))==0 and np.max(np.abs(x[-50:]))==0
    # 14s of the actual mastered music with a representative UI/game sequence.
    preview=render_preview(music,cues)
    report['preview']=stats(preview,SR)
    report['asset_total_bytes']=sum(p.stat().st_size for p in ASSETS.glob('*') if p.suffix in ['.ogg','.wav'])
    assert report['music_vorbis_decoded']['frame_count_matches']
    assert report['music_vorbis_decoded']['loop_seam_step']<.01
    assert all(v['finite'] and v['clipped_samples']==0 and v['peak_dbfs']<-.8 for v in [report['music_master'],report['music_vorbis_decoded'],report['preview'],*report['sfx'].values()])
    (SOURCE/'audio-validation.json').write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report,indent=2))
if __name__=='__main__': main()
