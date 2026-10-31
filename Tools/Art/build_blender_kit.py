"""Rebuild editable M2 kit + static FBX exports. Run with Blender --background --python this_file."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Blender'
EXPORT = ROOT / 'Assets/TetherRush/Resources/ArtModels'
for path in (SOURCE, EXPORT): path.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for d in list(bpy.data.collections):
    if d.name != 'Collection': bpy.data.collections.remove(d)
scene = bpy.context.scene
bpy.context.preferences.filepaths.save_version=0
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
colors = {'Body': (.227,.294,.384,1), 'Dark': (.055,.098,.161,1), 'Accent': (.259,.875,.961,1)}
mats = {}
for name, col in colors.items():
    m=bpy.data.materials.new(name); m.diffuse_color=col; m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF'); bs.inputs['Base Color'].default_value=col
    bs.inputs['Metallic'].default_value=.45; bs.inputs['Roughness'].default_value=.3
    mats[name]=m
assets={}
active=None

def mesh_obj(obj, name, role):
    obj.name=name
    for c in list(obj.users_collection): c.objects.unlink(obj)
    active.objects.link(obj)
    obj.data.materials.clear(); obj.data.materials.append(mats[role])
    return obj

def box(name, center, size, role='Body', bevel=.035):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center)
    o=mesh_obj(bpy.context.object,name,role); o.dimensions=size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        m=o.modifiers.new('Machined edge chamfer','BEVEL'); m.width=bevel; m.segments=1
        bpy.ops.object.modifier_apply(modifier=m.name)
    return o

def torus(name, radius, minor, role='Accent', rotation=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=radius, minor_radius=minor, major_segments=16, minor_segments=6, rotation=rotation)
    return mesh_obj(bpy.context.object,name,role)

def begin(name):
    global active
    active=bpy.data.collections.new(name); scene.collection.children.link(active)
    root=bpy.data.objects.new(name,None); active.objects.link(root)
    assets[name]={'root':root, 'collection':active}

def join_parts(parts, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join(); parts[0].name=name
    return parts[0]

def finish(name):
    # Keep only animation contracts separate. Consolidated static submeshes avoid
    # a renderer per rivet/trim, especially across the repeated runway tiles.
    parts=[o for o in active.objects if o.type=='MESH']
    if name=='TrackTile':
        join_parts([o for o in parts if o.name!='Deck'],'TrackTrims')
    elif name not in ['EnergyGate','BreakableBlock']:
        join_parts(parts,name+'Mesh')
    bpy.context.view_layer.update()
    root=assets[name]['root']; meshes=[o for o in active.objects if o.type=='MESH']
    coords=[o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
    lo=Vector(tuple(min(c[i] for c in coords) for i in range(3)))
    hi=Vector(tuple(max(c[i] for c in coords) for i in range(3)))
    center=(lo+hi)/2; span=hi-lo
    for o in meshes:
        world=o.matrix_world.copy()
        for v in o.data.vertices:
            p=world @ v.co-center
            v.co=tuple(p[i]/span[i] for i in range(3))
        o.location=(0,0,0); o.rotation_euler=(0,0,0); o.scale=(1,1,1)
        o.parent=root
        bpy.context.view_layer.objects.active=o
        m=o.modifiers.new('Export triangles','TRIANGULATE'); bpy.ops.object.modifier_apply(modifier=m.name)
    assets[name]['meshes']=meshes

begin('Core')
bpy.ops.mesh.primitive_uv_sphere_add(segments=20, ring_count=10, radius=.485)
o=mesh_obj(bpy.context.object,'CoreShell','Body')
for p in o.data.polygons: p.use_smooth=True
torus('EquatorBand',.477,.023,'Dark')
torus('MeridianBand',.477,.016,'Dark',(math.pi/2,0,0))
for z in [-.475,.475]:
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=.13, depth=.03, location=(0,0,z))
    mesh_obj(bpy.context.object,'EmitterBezel','Dark')
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=.085, depth=.018, location=(0,0,z*1.035))
    mesh_obj(bpy.context.object,'EmitterLens','Accent')
finish('Core')

begin('TrackTile')
box('Deck',(0,0,-.025),(1,1,.10),'Body',.012)
for x in [-.465,.465]:
    box('RailHousing',(x,0,.032),(.07,1,.035),'Dark',.007)
    box('RailInlay',(x,0,.053),(.013,.90,.008),'Accent',.002)
for x in [-.395,.395]: box('RunwayBorder',(x,0,.027),(.014,1,.004),'Dark',0)
for y in [-.475,.475]: box('TileJoint',(0,y,.027),(.78,.008,.004),'Dark',0)
for x in [-.35,.35]:
    for y in [-.35,.35]: box('EdgeTick',(x,y,.030),(.035,.09,.006),'Accent',.002)
finish('TrackTile')

begin('Barrier')
box('ArmoredMass',(0,0,0),(1,.48,1),'Body',.08)
for x in [-.28,.28]: box('RecessPanel',(x,-.248,0),(.32,.035,.66),'Dark',.025)
box('WarningStrip',(0,-.273,.33),(.76,.025,.05),'Accent',.008)
for z in [-.45,.45]: box('CrownGuard',(0,0,z),(1,.55,.10),'Dark',.025)
for x in [-.44,.44]: box('CornerShield',(x,-.27,0),(.075,.09,.74),'Body',.016)
box('StructuralSpine',(0,-.28,0),(.055,.07,.74),'Body',.01)
finish('Barrier')

begin('MovingWall')
box('SlidingPanel',(0,0,0),(1,.36,1),'Body',.06)
box('PanelInset',(0,-.19,0),(.72,.035,.62),'Dark',.03)
for x in [-.31,0,.31]:
    o=box('DirectionSlash',(x,-.22,0),(.055,.025,.38),'Accent',.008); o.rotation_euler[1]=-.35
for z in [-.45,.45]: box('SlideHousing',(0,0,z),(1,.45,.10),'Dark',.02)
for x in [-.44,.44]: box('SliderRunner',(x,-.20,0),(.075,.075,.75),'Body',.015)
finish('MovingWall')

begin('RotatingArm')
box('Sweeper',(0,0,0),(1,.14,.18),'Body',.045)
for x in [-.43,.43]: box('EndGuard',(x,0,0),(.14,.23,.3),'Dark',.03)
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=.13, depth=.32)
mesh_obj(bpy.context.object,'Axle','Accent')
for x in [-.43,.43]: box('TipSignal',(x,-.123,0),(.055,.016,.17),'Accent',.006)
for x in [-.23,.23]: box('BeamInset',(x,-.078,0),(.22,.014,.06),'Dark',.008)
finish('RotatingArm')

begin('EnergyGate')
# Door union is closed across the whole inner width; no decorative pass-through hole.
for x, name in [(-.235,'DoorLeft'),(.235,'DoorRight')]:
    parts=[box(name,(x,0,0),(.47,.25,.84),'Body',.02),
           box(name+'Inset',(x,-.14,0),(.32,.035,.57),'Dark',.02),
           box(name+'EnergyLine',(x,-.165,0),(.045,.02,.42),'Accent',.005)]
    for z in [-.32,.32]: parts.append(box('DoorHinge',(x,-.14,z),(.29,.07,.055),'Body',.01))
    join_parts(parts,name)
frame=[]
for x in [-.49,.49]: frame.append(box('FrameSide',(x,0,0),(.1,.34,1),'Dark',.02))
for z in [-.46,.46]: frame.append(box('FrameCap',(0,0,z),(.95,.34,.1),'Dark',.02))
join_parts(frame,'Frame')
finish('EnergyGate')

begin('BreakableBlock')
# Four closed quadrant shards with narrow recessed seams; full block silhouette.
for i,(x,z) in enumerate([(-.25,-.25),(.25,-.25),(-.25,.25),(.25,.25)]):
    parts=[box('Shard'+str(i+1),(x,0,z),(.499,.55,.499),'Body',.015),
           box('Shard'+str(i+1)+'Inset',(x,-.285,z),(.32,.025,.31),'Dark',.025),
           box('TargetQuarter',(x*.45,-.31,z*.45),(.12,.025,.12),'Accent',.015)]
    parts.append(box('ShardArmor',(x,-.31,z*1.5),(.29,.03,.05),'Body',.008))
    join_parts(parts,'Shard'+str(i+1))
finish('BreakableBlock')

begin('EnergyPickup')
# Link pickup uses a circular socket, distinctly different from a faceted bonus.
torus('EnergySocket',.34,.07,'Dark',(math.pi/2,0,0))
box('ChargeCell',(0,0,0),(.23,.16,.55),'Body',.045)
box('ChargeLens',(0,-.095,0),(.10,.025,.35),'Accent',.016)
torus('InductionRim',.34,.022,'Accent',(math.pi/2,0,0))
for z in [-.28,.28]: box('CellCap',(0,0,z),(.27,.21,.07),'Dark',.012)
finish('EnergyPickup')

begin('BonusCrystal')
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=.5)
o=mesh_obj(bpy.context.object,'FacetedCrystal','Body'); o.scale=(.68,.68,1)
box('CrystalCollar',(0,0,0),(.6,.6,.08),'Dark',.04)
for z in [-.25,.25]:
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=.19, depth=.025, location=(0,0,z))
    mesh_obj(bpy.context.object,'FacetBand','Accent')
finish('BonusCrystal')

begin('PulseBadge')
torus('TargetRing',.37,.06,'Dark',(math.pi/2,0,0))
torus('TargetInlay',.37,.02,'Accent',(math.pi/2,0,0))
for x in [-.16,.16]:
    o=box('PulseBolt',(x,0,0),(.07,.09,.45),'Accent',.008); o.rotation_euler[1]=-.4
finish('PulseBadge')

begin('FinishMarker')
for x in [-.44,.44]: box('FinishPylon',(x,0,0),(.12,.25,1),'Dark',.025)
box('FinishHeader',(0,0,.4),(.84,.22,.17),'Body',.025)
for x in [-.3,-.1,.1,.3]: box('FinishStripe',(x,-.13,.4),(.085,.025,.1),'Accent',.008)
for x in [-.44,.44]: box('FinishInlay',(x,-.14,.04),(.03,.02,.64),'Accent',.005)
finish('FinishMarker')

begin('SplitPylon')
# Entire core-height silhouette is closed and solid; channels are surface details.
box('PylonMass',(0,0,0),(1,.60,1),'Body',.06)
for z in [-.44,.44]: box('Foundation',(0,0,z),(1,.68,.12),'Dark',.025)
for x in [-.30,.30]:
    box('RecessedFace',(x,-.317,0),(.29,.035,.65),'Dark',.025)
    box('FaceSignal',(x,-.342,.21),(.20,.014,.035),'Accent',.006)
    box('CornerArmor',(x*1.45,-.34,0),(.09,.12,.71),'Body',.02)
box('StructuralSpine',(0,-.35,0),(.13,.10,.78),'Dark',.025)
box('VerticalSignal',(0,-.409,0),(.032,.015,.54),'Accent',.005)
finish('SplitPylon')

begin('ChevronBarrier')
box('ClosedArmoredBulk',(0,0,0),(1,.58,1),'Body',.06)
for z in [-.44,.44]: box('CrownGuard',(0,0,z),(1,.64,.12),'Dark',.025)
box('RecessedFace',(0,-.31,0),(.82,.035,.67),'Dark',.025)
for x,angle in [(-.16,-.55),(.16,.55)]:
    o=box('ChevronInlay',(x,-.338,.01),(.055,.02,.49),'Accent',.006); o.rotation_euler[1]=angle
for x in [-.44,.44]: box('EdgeShield',(x,-.32,0),(.10,.12,.66),'Body',.017)
finish('ChevronBarrier')

begin('RelayPylon')
box('AnchorFoot',(0,0,-.43),(.62,.62,.14),'Dark',.04)
box('RelayColumn',(0,0,-.02),(.36,.36,.81),'Body',.04)
for x in [-.19,.19]: box('ServiceChannel',(x,0,-.05),(.055,.28,.53),'Dark',.009)
box('BeaconHousing',(0,0,.34),(.53,.48,.25),'Dark',.04)
for y in [-.25,.25]: box('BeaconLens',(0,y,.34),(.35,.025,.11),'Accent',.015)
box('BeaconCap',(0,0,.47),(.55,.50,.06),'Body',.012)
finish('RelayPylon')

profile=dict(use_selection=True, object_types={'MESH','EMPTY'}, axis_forward='-Z', axis_up='Y', global_scale=1., apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', use_space_transform=True, bake_space_transform=False, bake_anim=False, use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False, path_mode='AUTO')
props=bpy.ops.export_scene.fbx.get_rna_type().properties
assert '-Z' in props['axis_forward'].enum_items.keys() and 'Y' in props['axis_up'].enum_items.keys()
manifest={'blender_version':bpy.app.version_string,'profile':{k:(sorted(v) if isinstance(v,set) else v) for k,v in profile.items()},'assets':[]}
for name,a in assets.items():
    bpy.ops.object.select_all(action='DESELECT')
    a['root'].select_set(True)
    for o in a['meshes']: o.select_set(True)
    bpy.context.view_layer.objects.active=a['root']
    dest=EXPORT/(name+'.fbx'); bpy.ops.export_scene.fbx(filepath=str(dest),**profile)
    tri=sum(len(o.data.polygons) for o in a['meshes'])
    bpy.context.view_layer.update()
    coords=[o.matrix_world @ Vector(c) for o in a['meshes'] for c in o.bound_box]
    bounds=[max(v[i] for v in coords)-min(v[i] for v in coords) for i in range(3)]
    assert all(abs(n-1)<1e-5 for n in bounds),(name,bounds)
    manifest['assets'].append({'id':name,'triangles':tri,'mesh_count':len(a['meshes']),'bounds_source_xyz':bounds,'pivot':[0,0,0],'root_scale':[1,1,1],'material_slots':sorted(set(m.name for o in a['meshes'] for m in o.data.materials)),'children':[o.name for o in a['meshes']],'bytes':dest.stat().st_size})
    if name=='TrackTile': manifest['assets'][-1]['deck_top_normalized_unity_y']=.25757575757575757
    assert tri <= 2200, (name,tri)
assert sum(a['triangles'] for a in manifest['assets']) < 12000
(SOURCE/'export_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
for name,a in assets.items(): a['collection'].hide_viewport=name!='Core'; a['collection'].hide_render=True
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'TetherRushKit.blend'))

# Temporary render-only duplicates; never included in FBX or editable source save.
preview=bpy.data.collections.new('ContactSheet'); scene.collection.children.link(preview)
role_colors={'Core':(1,.706,.561,1),'TrackTile':(.129,.196,.278,1),'Barrier':(.431,.192,.271,1),'MovingWall':(.431,.192,.271,1),'RotatingArm':(.431,.192,.271,1),'EnergyGate':(.863,.639,.286,1),'BreakableBlock':(.863,.639,.286,1),'EnergyPickup':(.227,.294,.384,1),'BonusCrystal':(.710,.643,1,1),'PulseBadge':(.863,.639,.286,1),'FinishMarker':(.227,.294,.384,1),'SplitPylon':(.431,.192,.271,1),'ChevronBarrier':(.431,.192,.271,1),'RelayPylon':(.227,.294,.384,1)}
for idx,(name,a) in enumerate(assets.items()):
    x=(idx%4-1.5)*2.4; y=(1.5-idx//4)*2.6
    for obj in a['meshes']:
        o=obj.copy(); o.data=obj.data.copy(); o.parent=None; preview.objects.link(o); o.location=(x,y,0)
        o.rotation_euler=(math.radians(-63),0,math.radians(-16))
        if name=='TrackTile': o.scale=(1.6,1.5,.15)
        elif name in ['Barrier','MovingWall','EnergyGate','BreakableBlock','FinishMarker','PulseBadge','ChevronBarrier','SplitPylon']: o.scale=(1.4,.45,1.25)
        elif name=='RotatingArm': o.scale=(1.6,.5,.35)
        elif name=='EnergyPickup': o.scale=(.85,.32,.85)
        for i,m in enumerate(o.data.materials):
            if m.name in ['Body','Accent']:
                material=m.copy()
                color=role_colors[name] if m.name=='Body' else ((.953,.420,.490,1) if name in ['Barrier','MovingWall','RotatingArm','SplitPylon','ChevronBarrier'] else (1,.847,.522,1) if name in ['EnergyGate','BreakableBlock','PulseBadge'] else (.259,.875,.961,1))
                material.diffuse_color=color; o.data.materials[i]=material
    curve=bpy.data.curves.new(name+'Label','FONT'); curve.body=name; curve.align_x='CENTER'; curve.size=.19
    label=bpy.data.objects.new(name+'Label',curve); preview.objects.link(label); label.location=(x,y-1.,.3); curve.materials.append(mats['Accent'])
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'; scene.display.shading.studio_light='paint.sl'
scene.display.shading.color_type='MATERIAL'; scene.display.shading.show_shadows=False; scene.display.shading.show_cavity=True
scene.display.shading.cavity_type='BOTH'; scene.display.shading.background_type='WORLD'; scene.world.color=(.012,.018,.03)
camdata=bpy.data.cameras.new('PreviewCamera'); cam=bpy.data.objects.new('PreviewCamera',camdata); preview.objects.link(cam)
cam.location=(0,-.3,15); camdata.type='ORTHO'; camdata.ortho_scale=11.5; scene.camera=cam
scene.render.resolution_x=1800; scene.render.resolution_y=1800; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'; scene.render.filepath=str(SOURCE/'preview.png')
bpy.ops.render.render(write_still=True)
print('M2_ASSET_BUILD_COMPLETE '+json.dumps(manifest))
