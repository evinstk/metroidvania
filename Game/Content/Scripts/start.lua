start_coroutine(function()
    -- Ripp
    dialog('Greetings!')
    dialog('Well, the Divine Beast probably won\'t head in this direction... But we should be on alert, just in case.')

    local options = {
        'Gerudo Town?',
        'Divine Beast?',
        'Good-bye.'
    }

    local selection, index = dialog{
        line='If you\'re thinking of going anywhere near Gerudo Town, you should definitely be careful, too.',
        options=options,
    }

    local function gerudo_town()
        -- Ripp
        dialog('Gerudo Town is to the southwest. It\'s the biggest town in the area!')
        dialog('It\'s famous for trade and also for staying active and vibrant all night long.')
        return dialog{
            line='However, there is a law forbidding voes--males--from entering the city.',
            options=options,
        }
    end

    local function divine_beast()
        dialog('You don\'t know about Divine Beast Vah Noboris?')
        dialog('Supposedly, it is the guardian deity for the Gerudo people, but it suddenly started acting up a while back.')
        dialog('There\'s nothing we can do to stop it, either... It\'s protected by a fierce sandstorm and intense lightning.')
        return dialog{
            line='Though it hasn\'t yet, it could easily head toward Gerudo Town or this oasis. I do my best to keep an eye on it from here.',
            options=options,
        }
    end

    while selection == 'Gerudo Town?' or selection == 'Divine Beast?' do
        table.remove(options, index)
        options = #options == 1 and { 'Thank you.' } or options
        if selection == 'Gerudo Town?' then
            selection, index = gerudo_town()
        elseif selection == 'Divine Beast?' then
            selection, index = divine_beast()
        end
    end

    if selection == 'Thank you.' then
        dialog('Hah. I don\'t need any thanks. This is my job after all.')
    else
        dialog('Be careful out there.')
    end
    dialog('You know, the desert is hot during the day and cold at night. It\'ll take the energy out of you in no time.')
    dialog('If you plan to head out into the desert, make sure you\'re prepared.')

    line()
end)





-- vars['test_contents'] = {
--     'Baton'
--     -- {'Baton', 1}
-- }
-- print(vars['test_contents'])
-- chest_contents = vars['test_contents']
-- chest_contents.add('Guard Baton', 1)
-- chest_contents.add('Blaster', 1)

-- start_coroutine(function()
--     -- wait_for(2)
--     -- line('Testing from script.')
--     -- interact()
--     -- line('Show for a little bit!')
--     -- interact()
--     -- line()
--     -- vars['cell_door_open'] = true
-- end)
