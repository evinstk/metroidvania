require 'common'

scene.set_fade(0, 0)
scene.set_letterbox(24, 0.5)
scene.show_hud(false)

local them = 'them'
local player_name = 'Samus'
local sworn_enemy = 'Yiga'
local shadowy = 'Cerberus'

local function center_dialog(line)
    return dialog({
        line=line,
        border=false,
        x = 80,
        y = 80,
    })
end

local function hal_speak(line, opts)
    return dialog({
        line=line,
        portrait=opts and opts.portrait or 'default_neutral',
        options=opts and opts.options or nil,
    })
end

start_coroutine(function()
    cutscene(function()
        wait_for(2)

        center_dialog('Is that ' .. them .. '?')
        center_dialog('Yes! Finally! After all this time...')
        center_dialog('Time to wake ' .. them .. ' up.')
        line()

        scene.set_fade(255, 3)
        wait_for(5)

        hal_speak('Alright. Releasing in 3, 2, 1...')
        line()

        local chamber = scene.find_entity('stasis_chamber')
        chamber.change_animation('stasis_chamber_opening', 'clamp_forever')
        wait(function()
            return not chamber.is_animation_running()
        end)

        chamber.change_animation('stasis_chamber_open', 'clamp_forever')
        local spawn_pos = scene.find_entity('player_spawn').get_position()
        local player = scene.create('player', spawn_pos.x, spawn_pos.y)
        player.possess()

        local selection = hal_speak(player_name .. ', can you read me?', {
            options={
                'Hal?',
                'What happened?',
                'Where am I?',
            },
        })

        if selection == 'Hal?' then
            hal_speak('Yeah, it\'s me. Been awhile. Well, maybe more for me than for you. You\'ve been frozen for some time.')
        elseif selection == 'What happened?' then
            hal_speak('You mean you don\'t remember? You were captured by the ' .. sworn_enemy .. '. I\'m afraid it\'s been months since we lost you.')
        elseif selection == 'Where am I?' then
            hal_speak('You\'re in the ' .. sworn_enemy .. '\'s stasis chambers. They had planned to deliver you to ' .. shadowy .. '.')
        end

        hal_speak('We can catch up later though. We need to get you out of there before they catch on.')
        hal_speak('I\'ve cracked into a layer of their security. I might be able to open some doors for you.')
        hal_speak('There should be a weapon cache in your vicinity. Get going, ' .. player_name .. '.')
    end)

    wait_for(2)
    scene.set_fade(0, 3)
    wait_for(3)

    cutscene({ keep_hud_off=true }, function()
        center_dialog('And then a really cool level happens.')
        center_dialog('Still working on it ;)')
        center_dialog('For now, enjoy a fun boss fight.\n\n  -- Tanner')
    end)

    scene.load_world('Training')
end)
