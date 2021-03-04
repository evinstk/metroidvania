local boss = find_entity('boss')

-- local boss_pos = boss.get_position()
-- print(boss_pos.x, boss_pos.y)

-- local player = find_entity('player')

-- start_coroutine(function()
--     wait(function()
--         return player.in_area('enter')
--     end)
--     line('Boss time!')
--     interact()
--     line()
-- end)

start_coroutine(function()
    wait(function()
        return boss.get_health() <= 0
    end)
    vars['boss_switch'] = true
end)
