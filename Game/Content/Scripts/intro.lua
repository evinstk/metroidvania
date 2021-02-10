local player = find_entity('hero')
local door_state = vars['prison-door-top-right']
local player_name = vars['player-name'].runtime_value

function begin()
    local start = find_entity('start')
    if (not player.collides(start))
    then
        print('exiting')
        return
    end

    wait_for(2)
    cutscene(player, function()
        dialog(player_name .. ', are you there? Come in.')
        dialog('We\'re busting you out today.')
        dialog('Give me a moment to hack into their systems. Hang tight.')
    end)

    wait_for(4)
    cutscene(player, function()
        dialog('Alright, that door should be opening in 3, 2, 1...')
        door_state.runtime_value = true
    end)

    local cell_exit = find_entity('cell-exit')
    wait_enter(player, cell_exit)
    cutscene(player, function()
        dialog('Yes! But you\'re pretty defenseless in your current state. We\'re gonna need to find you a weapon.')
        dialog('There should be a cache of confiscated weapons somewhere. See if you can find it.')
    end)
end

start_coroutine(begin)
