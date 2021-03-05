require 'common'

local player = find_entity('player')
local boss = find_entity('boss')

vars['boss_switch'] = true

start_coroutine(function()
    boss.possess()

    wait(function()
        return player.in_area('enter')
    end)

    player.possess()

    vars['boss_switch'] = false

    dialog{
        portrait='default_neutral',
        line='I cannot allow you to go any further.'
    }
    dialog{
        portrait='default_neutral',
        line='You\'ve been far too much of a nuisance to let you live.'
    }

    local selection, index = dialog{
        portrait='default_neutral',
        line='Any last words?',
        options={
            'You\'re not stopping me.',
            'Get out of my way!',
        }
    }

    if index == 1 then
        dialog{
            portrait='default_neutral',
            line='Oh I\'m not, huh? We\'ll see about that.'
        }
    else
        dialog{
            portrait='default_neutral',
            line='Hah! Over my dead body.'
        }
    end

    dialog{
        portrait='default_neutral',
        line='Steel yourself.'
    }

    player.release()
    boss.release()

    line()
end)

start_coroutine(function()
    wait(function()
        return boss.get_health() <= 0
    end)

    player.possess()

    dialog{
        portrait='default_neutral',
        line='No-o-o-o-o-o!'
    }
    line()

    player.release()

    vars['boss_switch'] = true
end)
