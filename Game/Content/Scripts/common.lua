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
