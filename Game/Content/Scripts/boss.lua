require 'common'

local player = scene.find_entity('player')
local boss = scene.find_entity('boss')

vars['boss_switch'] = true

local goblin_pitch = 0.8

start_coroutine(function()
    boss.possess()

    wait(function()
        return player.in_area('enter')
    end)

    cutscene({
        possess={ 'player', 'boss' },
    }, function()
        vars['boss_switch'] = false

        dialog{
            portrait='goblin_neutral',
            line='I cannot allow you to go any further.',
            pitch=goblin_pitch,
        }
        dialog{
            portrait='goblin_neutral',
            line='You\'ve been far too much of a nuisance to let you live.',
            pitch=goblin_pitch,
        }

        local selection, index = dialog{
            portrait='goblin_neutral',
            line='Any last words?',
            pitch=goblin_pitch,
            options={
                'You\'re not stopping me.',
                'Get out of my way!',
            }
        }

        if index == 1 then
            dialog{
                portrait='goblin_amused',
                line='Oh I\'m not, huh? We\'ll see about that.',
                pitch=goblin_pitch,
            }
        else
            dialog{
                portrait='goblin_amused',
                line='Hah! Over my dead body.',
                pitch=goblin_pitch,
            }
        end

        scene.load_music('Music', 'vs_solidus')
        scene.play_music()

        dialog{
            portrait='goblin_neutral',
            line='Steel yourself.',
            pitch=goblin_pitch,
        }
    end)
end)

start_coroutine(function()
    wait(function()
        return boss.get_health() <= 0
    end)

    scene.stop_music(true)
    cutscene(function()
        dialog{
            portrait='goblin_terror',
            line='No-o-o-o-o-o!',
            pitch=goblin_pitch,
        }
    end)

    vars['boss_switch'] = true
end)
