--- coroutines
function wait_for(duration)
    local elapsed = 0
    while elapsed < duration
    do
        elapsed = elapsed + delta_time
        coroutine.yield()
    end
end

function wait(fn)
    while not fn() do
        coroutine.yield()
    end
end

function translate(entity, x, y, t)
    local orig_pos = entity.get_position()
    local elapsed = 0
    repeat
        elapsed = elapsed + delta_time
        if elapsed >= t then
            entity.set_position(orig_pos.x + x, orig_pos.y + y)
        else
            local pct = elapsed / t
            entity.set_position(
                orig_pos.x + x * pct,
                orig_pos.y + y * pct)
        end
        coroutine.yield()
    until elapsed >= t
end

--- predicates
function in_area(entity, area_name)
    return function()
        return entity.in_area(area_name)
    end
end

function animation_stopped(entity)
    return function()
        return not entity.is_animation_running()
    end
end

--- cutscenes
function interact()
    wait(function()
        return interaction_pressed
    end)
    interaction_pressed = false
end

function dialog(arg)
    local border = true
    if arg.border ~= nil then
        border = arg.border
    end
    line(
        type(arg) == 'string' and arg or arg.line,
        arg.portrait,
        arg.speaker,
        arg.options,
        arg.x or 10,
        arg.y or 10,
        border,
        arg.pitch or 0,
        arg.play_sound or true
    )
    interact()
    local option = read_dialog_option()
    return arg.options and arg.options[option], option
end

function cutscene(opts_overrides, fn)
    local opts = {
        letterbox=24,
        keep_hud_off=false,
        possess={ 'player' }
    }
    if fn == nil then
        -- only one arg given, use default opts
        fn = opts_overrides
    else
        for k,v in pairs(opts_overrides) do
            opts[k] = v
        end
    end

    for i, name in pairs(opts.possess) do
        local entity = scene.find_entity(name)
        if entity ~= nil then entity.possess() end
    end
    scene.set_letterbox(opts.letterbox, 0.5)
    scene.show_hud(false)

    camera.focus_on('player', 0.5)

    fn()
    line()

    camera.focus_on('player', 0.5)
    scene.set_letterbox(0, 0.5)
    wait_for(0.5)
    if not opts.keep_hud_off then scene.show_hud(true) end
    for i, name in pairs(opts.possess) do
        local entity = scene.find_entity(name)
        if entity ~= nil then entity.release() end
    end

    camera.release_focus()
end

function liftoff()
    local player = scene.find_entity('player')
    wait(in_area(player, 'beam'))

    cutscene({ keep_hud_off=true }, function()
        local player = scene.find_entity('player')
        translate(player, 0, -64, 2)
        player.destroy()

        local spacecraft = scene.find_entity('spacecraft')
        spacecraft.change_animation('liftoff', 'clamp_forever')
        wait(animation_stopped(spacecraft))
        spacecraft.change_animation('rise')
        translate(spacecraft, 0, -64, 3)
        spacecraft.change_animation('takeoff', 'clamp_forever')
        wait(animation_stopped(spacecraft))
        spacecraft.change_animation('fly')
        translate(spacecraft, -500, -100, 0.6)
        spacecraft.change_animation('distant_fly')
        translate(spacecraft, 0, 300, 0)
        translate(spacecraft, 500, -350, 2)

        scene.set_fade(0, 2)
        wait_for(2)
    end)
end
