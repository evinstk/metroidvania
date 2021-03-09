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

function in_area(entity, area_name)
    return function()
        return entity.in_area(area_name)
    end
end

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
        arg.options,
        arg.x or 10,
        arg.y or 10,
        border
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

    fn()
    line()

    scene.set_letterbox(0, 0.5)
    wait_for(0.5)
    if not opts.keep_hud_off then scene.show_hud(true) end
    for i, name in pairs(opts.possess) do
        local entity = scene.find_entity(name)
        if entity ~= nil then entity.release() end
    end
end
