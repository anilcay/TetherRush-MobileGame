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
colors = {'Body': (.06,.7,.8,1), 'Dark': (.026,.042,.065,1), 'Accent': (.65,.88,.92,1)}
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
    bpy.ops.mesh.primitive_torus_add(major_radius=radius, minor_radius=minor, major_segments=16, minor_segments=4, rotation=rotation)
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
bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=.485)
o=mesh_obj(bpy.context.object,'CoreShell','Body')
for p in o.data.polygons: p.use_smooth=True
torus('EquatorBand',.477,.023,'Dark')
torus('MeridianBand',.477,.023,'Accent',(math.pi/2,0,0))
finish('Core')

begin('TrackTile')
box('Deck',(0,0,0),(1,1,.12),'Dark')
for x in [-.46,.46]: box('EdgeRail',(x,0,.08),(.065,.96,.055),'Body',.012)
for y in [-.39,.39]: box('CrossSeam',(0,y,.065),(.77,.018,.012),'Accent',.002)
finish('TrackTile')

begin('Barrier')
box('ArmoredMass',(0,0,0),(1,.48,1),'Body',.08)
for x in [-.28,.28]: box('RecessPanel',(x,-.248,0),(.32,.035,.66),'Dark',.025)
box('WarningStrip',(0,-.273,.33),(.76,.025,.05),'Accent',.008)
finish('Barrier')

begin('MovingWall')
box('SlidingPanel',(0,0,0),(1,.36,1),'Body',.06)
box('PanelInset',(0,-.19,0),(.72,.035,.62),'Dark',.03)
for x in [-.31,0,.31]:
    o=box('DirectionSlash',(x,-.22,0),(.055,.025,.38),'Accent',.008); o.rotation_euler[1]=-.35
finish('MovingWall')

begin('RotatingArm')
box('Sweeper',(0,0,0),(1,.14,.18),'Body',.045)
for x in [-.43,.43]: box('EndGuard',(x,0,0),(.14,.23,.3),'Dark',.03)
bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=.13, depth=.32)
mesh_obj(bpy.context.object,'Axle','Accent')
finish('RotatingArm')

begin('EnergyGate')
# Door union is closed across the whole inner width; no decorative pass-through hole.
for x, name in [(-.235,'DoorLeft'),(.235,'DoorRight')]:
    parts=[box(name,(x,0,0),(.47,.25,.84),'Body',.02),
           box(name+'Inset',(x,-.14,0),(.32,.035,.57),'Dark',.02),
           box(name+'EnergyLine',(x,-.165,0),(.045,.02,.42),'Accent',.005)]
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
    join_parts(parts,'Shard'+str(i+1))
finish('BreakableBlock')

begin('EnergyPickup')
# Link pickup uses a circular socket, distinctly different from a faceted bonus.
torus('EnergySocket',.34,.09,'Body',(math.pi/2,0,0))
box('ChargeCell',(0,0,0),(.23,.16,.55),'Accent',.045)
finish('EnergyPickup')

begin('BonusCrystal')
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=.5)
o=mesh_obj(bpy.context.object,'FacetedCrystal','Body'); o.scale=(.68,.68,1)
box('CrystalCollar',(0,0,0),(.6,.6,.08),'Dark',.04)
finish('BonusCrystal')

begin('PulseBadge')
torus('TargetRing',.37,.06,'Body',(math.pi/2,0,0))
for x in [-.16,.16]:
    o=box('PulseBolt',(x,0,0),(.07,.09,.45),'Accent',.008); o.rotation_euler[1]=-.4
finish('PulseBadge')

begin('FinishMarker')
for x in [-.44,.44]: box('FinishPylon',(x,0,0),(.12,.25,1),'Dark',.025)
box('FinishHeader',(0,0,.4),(.84,.22,.17),'Body',.025)
for x in [-.3,-.1,.1,.3]: box('FinishStripe',(x,-.13,.4),(.085,.025,.1),'Accent',.008)
finish('FinishMarker')

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
    assert tri <= (800 if name=='Core' else 1200 if name in ['EnergyGate','BreakableBlock'] else 500), (name,tri)
(SOURCE/'export_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
for name,a in assets.items(): a['collection'].hide_viewport=name!='Core'; a['collection'].hide_render=True
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'TetherRushKit.blend'))

# Temporary render-only duplicates; never included in FBX or editable source save.
preview=bpy.data.collections.new('ContactSheet'); scene.collection.children.link(preview)
role_colors={'Core':(.05,.75,.9,1),'TrackTile':(.07,.35,.4,1),'Barrier':(.82,.06,.12,1),'MovingWall':(.82,.06,.12,1),'RotatingArm':(.82,.06,.12,1),'EnergyGate':(.9,.5,.05,1),'BreakableBlock':(.9,.5,.05,1),'EnergyPickup':(.03,.8,.9,1),'BonusCrystal':(.52,.16,.85,1),'PulseBadge':(.9,.5,.05,1),'FinishMarker':(.1,.8,.65,1)}
for idx,(name,a) in enumerate(assets.items()):
    x=(idx%4-1.5)*2.4; y=(1-idx//4)*2.6
    for obj in a['meshes']:
        o=obj.copy(); o.data=obj.data.copy(); o.parent=None; preview.objects.link(o); o.location=(x,y,0)
        o.rotation_euler=(math.radians(-63),0,math.radians(-16))
        if name=='TrackTile': o.scale=(1.6,1.5,.15)
        elif name in ['Barrier','MovingWall','EnergyGate','BreakableBlock','FinishMarker','PulseBadge']: o.scale=(1.4,.45,1.25)
        elif name=='RotatingArm': o.scale=(1.6,.5,.35)
        elif name=='EnergyPickup': o.scale=(.85,.32,.85)
        for i,m in enumerate(o.data.materials):
            if m.name=='Body':
                material=m.copy(); material.diffuse_color=role_colors[name]; o.data.materials[i]=material
    curve=bpy.data.curves.new(name+'Label','FONT'); curve.body=name; curve.align_x='CENTER'; curve.size=.19
    label=bpy.data.objects.new(name+'Label',curve); preview.objects.link(label); label.location=(x,y-1.,.3); curve.materials.append(mats['Accent'])
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'; scene.display.shading.studio_light='paint.sl'
scene.display.shading.color_type='MATERIAL'; scene.display.shading.show_shadows=False; scene.display.shading.show_cavity=True
scene.display.shading.cavity_type='BOTH'; scene.display.shading.background_type='WORLD'; scene.world.color=(.012,.018,.03)
camdata=bpy.data.cameras.new('PreviewCamera'); cam=bpy.data.objects.new('PreviewCamera',camdata); preview.objects.link(cam)
cam.location=(0,0,15); camdata.type='ORTHO'; camdata.ortho_scale=10.1; scene.camera=cam
scene.render.resolution_x=1800; scene.render.resolution_y=1500; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'; scene.render.filepath=str(SOURCE/'preview.png')
bpy.ops.render.render(write_still=True)
print('M2_ASSET_BUILD_COMPLETE '+json.dumps(manifest))
