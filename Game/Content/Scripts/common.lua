--- triggers

function onEnter(lhs, rhs)
	return function()
		return collides(lhs, rhs)
	end
end

function moveCommand(entity, dest)
	return function()
		move(entity, dest)
	end
end

function destroyCommand(entity)
	return function()
		destroy(entity)
	end
end

function onStart()
	return function()
		return true
	end
end

--- coroutines

function move_to(entity, dest)
    while true
	do
		if (collides(entity, dest))
		then
			stop(entity)
			-- return 0
			break
		end
        move(entity, dest)
        coroutine.yield()
    end
end

function dialog(line)
	speak(line)
	while true
	do
		if (interact())
		then
			-- clear speech
			speak()
			break
		end
		coroutine.yield()
	end
end

--- higher-order

function cutscene(player, fn)
	return function()
		disable(player)
		fn()
		enable(player)
	end
end
