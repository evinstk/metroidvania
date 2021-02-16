start_coroutine(function()
    wait_for(2)
    line('Testing from script.')
    interact()
    line('Show for a little bit!')
    interact()
    line()
    vars['cell_door_open'] = true
    vars['hud_prompt'] = 'Sup dude.'
end)
