print('playing korok_level')

-- local korok_sound = core.load_sound('FMOD/Common.bank', 'save')
local korok_sound = audio.load_sound('FMOD/Common.bank', 'korok_discovery')

on_event('found_korok_seed', function(flower)
    local pos = flower.get_position()
    scene.instantiate('korok', pos)
    korok_sound.start()
end)

on_event('korok_platform_hit', function(platform)
    print(platform.get_name())
end)

-- start_coroutine(function()
--     while true do
--         coroutine.yield()
--     end
-- end)
