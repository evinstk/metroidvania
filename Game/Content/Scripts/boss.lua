require 'common'

local player = find_entity('player')
local boss = find_entity('boss')

vars['boss_switch'] = true

start_coroutine(function()
    boss.possess()

    wait(function()
        return player.in_area('enter')
    end)

    scene.set_letterbox(24, 0.5)

    player.possess()

    vars['boss_switch'] = false

    dialog{
        portrait='goblin_neutral',
        line='I cannot allow you to go any further.',
    }
    dialog{
        portrait='goblin_neutral',
        line='You\'ve been far too much of a nuisance to let you live.'
    }

    local selection, index = dialog{
        portrait='goblin_neutral',
        line='Any last words?',
        options={
            'You\'re not stopping me.',
            'Get out of my way!',
        }
    }

    if index == 1 then
        dialog{
            portrait='goblin_amused',
            line='Oh I\'m not, huh? We\'ll see about that.'
        }
    else
        dialog{
            portrait='goblin_amused',
            line='Hah! Over my dead body.'
        }
    end

    dialog{
        portrait='goblin_neutral',
        line='Steel yourself.'
    }

    player.release()
    boss.release()
    scene.set_letterbox(0, 0.5)

    line()
end)

start_coroutine(function()
    wait(function()
        return boss.get_health() <= 0
    end)

    scene.set_letterbox(24, 0.5)
    player.possess()

    dialog{
        portrait='goblin_terror',
        line='No-o-o-o-o-o!'
    }
    line()

    player.release()
    scene.set_letterbox(0, 0.5)

    vars['boss_switch'] = true
end)
