print('playing korok_level')

on_event('found_korok_seed', function(korok_entity)
    local pos = korok_entity.get_position()
    scene.instantiate('korok', pos)
end)

-- start_coroutine(function()
--     while true do
--         coroutine.yield()
--     end
-- end)
