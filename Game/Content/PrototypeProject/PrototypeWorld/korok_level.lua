require 'Scripts/common'

print('playing korok_level')

-- local korok_sound = core.load_sound('FMOD/Common.bank', 'save')
local korok_sound = audio.load_sound('FMOD/Common.bank', 'korok_discovery')

on_event('found_korok_seed', function(flower)
    local pos = flower.get_position()
    scene.instantiate('korok', pos)
    korok_sound.start()
end)

local curr_trail, korok_pop
local function release_trail()
    if curr_trail then
        curr_trail.pause_emission()
        curr_trail = nil
        -- korok_pop.stop(0)
        korok_pop.release()
        -- korok_pop.clear_handle()
    end
end

on_event('korok_platform_hit', function(payload)
    release_trail()

    curr_trail = scene.instantiate(
        'korok_trail',
        payload.platform.get_position())
    korok_pop = audio.load_sound('FMOD/Common.bank', 'korok_pop')
    korok_pop.start()

    start_coroutine(function()
        local speed = payload.target - payload.platform.get_position()
        speed = normalize(speed)
        speed = speed * 300
        curr_trail.set_speed(speed)
        wait(function()
            return curr_trail == nil or (curr_trail.get_position() - payload.target).length_squared() < 16
        end)
        if curr_trail then
            scene.instantiate(
                'korok_ring',
                curr_trail.get_position(),
                { duration=payload.platform['duration'] })
        end
        release_trail()
    end)
end)

on_event('korok_ring_enter', function(entity)
    entity.destroy()
    scene.instantiate('korok', entity.get_position())
end)

-- start_coroutine(function()
--     while true do
--         coroutine.yield()
--     end
-- end)
