local player = findEntity("hero")

i = 1

first_run = true


cr = coroutine.create(cutscene(player, function()
    dialog("What's going on?")
end))

-- return function(entity)
--     -- coroutine.resume(cr)
--     while true
--     do
--         coroutine.yield()
--     end
-- end

return cutscene(player, function()
    dialog("What's going on?")
end)
