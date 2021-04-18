print('playing korok_level')

-- local korok_sound = core.load_sound('FMOD/Common.bank', 'save')
local korok_sound = audio.load_sound('FMOD/Common.bank', 'korok_discovery')

on_event('found_korok_seed', function(korok_entity)
    local pos = korok_entity.get_position()
    scene.instantiate('korok', pos)
    korok_sound.start()
end)

-- start_coroutine(function()
--     while true do
--         coroutine.yield()
--     end
-- end)
