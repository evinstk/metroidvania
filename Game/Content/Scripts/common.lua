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
