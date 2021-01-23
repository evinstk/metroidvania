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

function wait_enter(entity, area)
	while not entity.collides(area) do
		coroutine.yield()
	end
end

function move_to(entity, dest)
    while true
	do
		if (entity.collides(dest))
		then
			entity.stop()
			-- return 0
			break
		end
        entity.move(dest)
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

function wait_interaction(entity)
    while not entity.is_interacted() do
        coroutine.yield()
    end
end

function wait()
	coroutine.yield()
end

--- higher-order

function cutscene(player, fn)
	return function()
		player.set_enabled(false)
		fn()
		-- eat interaction
		wait()
		wait()
		player.set_enabled(true)
	end
end
