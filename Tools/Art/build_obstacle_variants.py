"""Reproducible, isolated obstacle kit. Blender --background --python this_file."""
import bpy, json, math, hashlib
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Blender/ObstacleVariants'
EXPORT = ROOT / 'Assets/TetherRush/Resources/ArtModels'
SOURCE.mkdir(parents=True, exist_ok=True)
IDS = ['ClampBarrier', 'CapacitorBank', 'HydraulicSlider', 'TurbineBeam']
old_hashes = {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in EXPORT.glob('*.fbx') if p.stem not in IDS}
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
scene = bpy.context.scene
scene.world = bpy.data.worlds.new('ObstacleVariantsWorld')
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
mats = {}
for name, col in {'Body':(.34,.40,.49,1), 'Dark':(.045,.075,.12,1), 'Accent':(.96,.25,.29,1)}.items():
    mat = bpy.data.materials.new(name); mat.diffuse_color = col
    mats[name] = mat
assets = {}
parts = []

def box(name, center, size, role='Body', bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center)
    obj = bpy.context.object; obj.name = name; obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mats[role])
    if bevel:
        mod = obj.modifiers.new('Machined bevel', 'BEVEL'); mod.width=bevel; mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    parts.append(obj)
    return obj

def cylinder(name, center, radius, depth, role, vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=center)
    obj=bpy.context.object; obj.name=name; obj.data.materials.append(mats[role]); parts.append(obj)
    return obj

def begin(name):
    global parts
    parts=[]
    # Exact unchamfered rectangular contact slab. Every X/Y point is filled
    # from normalized vertical -.5 to +.16, including the gameplay core band.
    box('FullContactSlab',(0,0,-.17),(1,1,.66),'Body')
    # Front warning band lies on, rather than outside, the footprint.
    box('DangerBand',(0,-.494,.015),(.96,.012,.11),'Accent')

def finish(name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join(); obj=parts[0]; obj.name=name+'Mesh'
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    # Joining can repeat material slots. Consolidate to the exact three roles.
    roles=[obj.data.materials[p.material_index].name for p in obj.data.polygons]
    obj.data.materials.clear()
    for role in ['Body','Dark','Accent']: obj.data.materials.append(mats[role])
    for p,role in zip(obj.data.polygons,roles): p.material_index=['Body','Dark','Accent'].index(role)
    mod=obj.modifiers.new('Export triangles','TRIANGULATE'); bpy.ops.object.modifier_apply(modifier=mod.name)
    root=bpy.data.objects.new(name,None); scene.collection.objects.link(root); obj.parent=root
    collection=bpy.data.collections.new(name); scene.collection.children.link(collection)
    for item in [root,obj]:
        for old_collection in list(item.users_collection): old_collection.objects.unlink(item)
        collection.objects.link(item)
    assets[name]=(root,obj)

begin('ClampBarrier')
box('RecessedTop',(0,0,.19),(.80,.92,.06),'Dark',.012)
for x in [-.35,.35]:
    box('ClampJaw',(x,.06,.32),(.30,.84,.32),'Body',.035)
    box('JawDangerCap',(x,.06,.49),(.23,.73,.02),'Accent')
    box('JawFace',(x,-.367,.31),(.20,.012,.18),'Dark',.01)
box('LockedCenterRail',(0,0,.245),(.12,.92,.11),'Dark',.012)
for y in [-.28,0,.28]: box('RailNotch',(0,y,.305),(.12,.07,.012),'Accent')
finish('ClampBarrier')

begin('CapacitorBank')
box('BankSocket',(0,0,.20),(.94,.90,.08),'Dark',.018)
for x in [-.30,0,.30]:
    cylinder('CapacitorPod',(x,0,.34),.14,.28,'Body')
    cylinder('PodInsulator',(x,0,.465),.12,.05,'Dark')
    cylinder('ChargeTerminal',(x,0,.495),.075,.01,'Accent')
    for y in [-.136,.136]: box('PodSignal',(x,y,.34),(.055,.015,.18),'Accent')
for y in [-.40,.40]: box('BusBar',(0,y,.27),(.92,.08,.14),'Dark',.012)
finish('CapacitorBank')

begin('HydraulicSlider')
box('Platen',(0,-.19,.28),(.92,.52,.24),'Body',.025)
box('PlatenInset',(0,-.455,.29),(.72,.012,.15),'Dark',.01)
for x in [-.30,.30]:
    obj=cylinder('PistonHousing',(x,.21,.285),.13,.43,'Dark')
    obj.rotation_euler[0]=math.pi/2
    obj=cylinder('PistonRod',(x,-.015,.285),.065,.26,'Body')
    obj.rotation_euler[0]=math.pi/2
    box('LinearGuide',(x,.10,.455),(.12,.72,.07),'Body',.012)
    box('GuideSignal',(x,.10,.495),(.055,.60,.01),'Accent')
for x in [-.16,.16]:
    o=box('TranslationMark',(x,-.464,.29),(.04,.008,.12),'Accent'); o.rotation_euler[1]=-.35
finish('HydraulicSlider')

begin('TurbineBeam')
box('ContinuousBeam',(0,0,.24),(1,1,.16),'Body',.025)
for y in [-.38,.38]: box('BeamDangerBand',(0,y,.325),(.95,.12,.01),'Accent')
cylinder('TurbineHousing',(0,0,.37),.31,.10,'Dark',16)
cylinder('AxleCap',(0,0,.46),.13,.06,'Body',12)
cylinder('AxleSignal',(0,0,.495),.075,.01,'Accent',12)
for i in range(8):
    angle=i*math.tau/8
    obj=box('SurfaceCoolingFin',(.215*math.cos(angle),.215*math.sin(angle),.445),(.14,.038,.05),'Body')
    obj.rotation_euler[2]=angle+.4
finish('TurbineBeam')

profile=dict(use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',global_scale=1.,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',use_space_transform=True,bake_space_transform=False,bake_anim=False,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='AUTO')
manifest={'blender_version':bpy.app.version_string,'profile':{k:sorted(v) if isinstance(v,set) else v for k,v in profile.items()},'old_fbx_sha256':old_hashes,'assets':[]}
for name,(root,obj) in assets.items():
    bpy.context.view_layer.update()
    coords=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    lo=[min(p[i] for p in coords) for i in range(3)]; hi=[max(p[i] for p in coords) for i in range(3)]
    assert all(abs(lo[i]+.5)<1e-5 and abs(hi[i]-.5)<1e-5 for i in range(3)),(name,lo,hi)
    tri=len(obj.data.polygons); assert tri<=1000,(name,tri)
    bpy.ops.object.select_all(action='DESELECT'); root.select_set(True); obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    dest=EXPORT/(name+'.fbx'); bpy.ops.export_scene.fbx(filepath=str(dest),**profile)
    manifest['assets'].append({'id':name,'triangles':tri,'mesh_count':1,'material_slots':['Body','Dark','Accent'],'bounds_source_xyz':[1,1,1],'center':[0,0,0],'root_scale':[1,1,1],'contact_slab_source_z':[-.5,.16],'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'bytes':dest.stat().st_size})
assert all(hashlib.sha256((EXPORT/name).read_bytes()).hexdigest()==sha for name,sha in old_hashes.items())
(SOURCE/'export_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
for name in assets:
    bpy.data.collections[name].hide_viewport=name!='ClampBarrier'
    bpy.data.collections[name].hide_render=True
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'ObstacleVariants.blend'))

# Preview duplicates only: source save and FBX remain in local normalized space.
for root,obj in assets.values(): obj.hide_render=True
for i,(name,(root,obj)) in enumerate(assets.items()):
    x=(i%2-.5)*3.8; y=(.5-i//2)*3.4
    dup=obj.copy(); dup.data=obj.data.copy(); dup.parent=None; scene.collection.objects.link(dup)
    dup.hide_render=False; dup.location=(x,y,0)
    dup.rotation_euler=(math.radians(-48),0,math.radians(-18))
    dup.scale=(2.5,1.2,1.3) if name!='TurbineBeam' else (3.0,1.15,.7)
    curve=bpy.data.curves.new(name+'Label','FONT'); curve.body=name; curve.align_x='CENTER'; curve.size=.23
    label=bpy.data.objects.new(name+'Label',curve); scene.collection.objects.link(label); label.location=(x,y-1.25,.4); curve.materials.append(mats['Accent'])
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'; scene.display.shading.studio_light='paint.sl'
scene.display.shading.color_type='MATERIAL'; scene.display.shading.show_shadows=False
scene.display.shading.show_cavity=True; scene.display.shading.cavity_type='BOTH'
scene.display.shading.background_type='WORLD'; scene.world.color=(.018,.025,.04)
camdata=bpy.data.cameras.new('PreviewCamera'); cam=bpy.data.objects.new('PreviewCamera',camdata); scene.collection.objects.link(cam)
cam.location=(0,0,15); camdata.type='ORTHO'; camdata.ortho_scale=8.4; scene.camera=cam
scene.render.resolution_x=1800; scene.render.resolution_y=1600; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'; scene.render.filepath=str(SOURCE/'preview.png')
bpy.ops.render.render(write_still=True)
print('OBSTACLE_VARIANTS_BUILD_PASS '+json.dumps(manifest))
