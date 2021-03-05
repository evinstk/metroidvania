require 'common'

function gerudo_dialog()
    -- Ripp
    dialog{
        portrait='default_neutral',
        line='Greetings!',
    }
    dialog{
        portrait='default_neutral',
        line='Well, the Divine Beast probably won\'t head in this direction... But we should be on alert, just in case.',
    }

    local town_option = 'Gerudo Town?'
    local beast_option = 'Divine Beast? What\'s that?'
    local options = {
        town_option,
        beast_option,
        'Good-bye.'
    }

    local selection, index = dialog{
        portrait='default_neutral',
        line='If you\'re thinking of going anywhere near Gerudo Town, you should definitely be careful, too.',
        options=options,
    }

    local function gerudo_town()
        -- Ripp
        dialog{
            portrait='default_neutral',
            line='Gerudo Town is to the southwest. It\'s the biggest town in the area!',
        }
        dialog{
            portrait='default_neutral',
            line='It\'s famous for trade and also for staying active and vibrant all night long.',
        }
        return dialog{
            portrait='default_neutral',
            line='However, there is a law forbidding voes--males--from entering the city.',
            options=options,
        }
    end

    local function divine_beast()
        dialog{
            portrait='default_neutral',
            line='You don\'t know about Divine Beast Vah Noboris?',
        }
        dialog{
            portrait='default_neutral',
            line='Supposedly, it is the guardian deity for the Gerudo people, but it suddenly started acting up a while back.',
        }
        dialog{
            portrait='default_neutral',
            line='There\'s nothing we can do to stop it, either... It\'s protected by a fierce sandstorm and intense lightning.',
        }
        return dialog{
            portrait='default_neutral',
            line='Though it hasn\'t yet, it could easily head toward Gerudo Town or this oasis. I do my best to keep an eye on it from here.',
            options=options,
        }
    end

    while selection == town_option or selection == beast_option do
        table.remove(options, index)
        options = #options == 1 and { 'Thank you.' } or options
        if selection == town_option then
            selection, index = gerudo_town()
        elseif selection == beast_option then
            selection, index = divine_beast()
        end
    end

    if selection == 'Thank you.' then
        dialog{
            portrait='default_neutral',
            line='Hah. I don\'t need any thanks. This is my job after all.'
        }
    else
        dialog{
            portrait='default_neutral',
            line='Be careful out there.'
        }
    end
    dialog{
        portrait='default_neutral',
        line='You know, the desert is hot during the day and cold at night. It\'ll take the energy out of you in no time.',
    }
    dialog{
        portrait='default_neutral',
        line='If you plan to head out into the desert, make sure you\'re prepared.'
    }

    line()
end
