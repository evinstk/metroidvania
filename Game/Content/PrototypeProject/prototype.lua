screen.set_size(1920, 1080)
scene.set_design_resolution(480, 270)

scene_settings.default_camera_target = 'botw_link'

audio.load_bank('FMOD/Master.bank')
audio.load_bank('FMOD/Master.strings.bank')

scene.load_world(
    'PrototypeProject/PrototypeProject.ogmo',
    'PrototypeProject/PrototypeWorld',
    {
        layers={
            terrain={
                render_layer=-48,
                collision_mask=MASK_TERRAIN,
            },
        },
    })
scene.run_room(vec2(16.2, 8))
