print('playing korok_level')

on_event('found_korok_seed', function(korok_entity)
    print(korok_entity.get_name())
end)

-- start_coroutine(function()
--     while true do
--         coroutine.yield()
--     end
-- end)
