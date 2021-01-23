local sage = find_entity('Sage')
local player = find_entity('hero')
local bypassSage = find_entity('bypassSage')

local talked_to_sage = false
local annoyed_sage = false

start_coroutine(function ()
    wait_interaction(sage)
    talked_to_sage = true
    local loop_dialog
    if not annoyed_sage
    then
        cutscene(player, function()
            sage.talk()
            dialog("Ah! Come to test your mettle against the demons, have you?")
            dialog("Just flip the switch there and drop down.")
            dialog("Good luck! Would hate to see another one of you adventuring types fall...")
            sage.rest()
        end)()

        loop_dialog = cutscene(player, function()
            sage.talk()
            dialog("Best be on your way, then.")
            sage.rest()
        end)
    else
        cutscene(player, function()
            sage.talk()
            dialog("Oh NOW you want to talk, do you?")
            dialog("Well I'm afraid now I'm not in a chatty mood.")
            dialog("So I think you best skedaddle.")
            sage.rest()
        end)()

        loop_dialog = cutscene(player, function()
            sage.talk()
            dialog("Go on, then.")
            sage.rest()
        end)
    end

    while true
    do
        wait_interaction(sage)
        loop_dialog()
    end
end)

start_coroutine(function ()
    wait_enter(player, bypassSage)
    print('suup')
    if talked_to_sage == false
    then
        annoyed_sage = true
        cutscene(player, function()
            sage.talk()
            dialog("That's fine. I didn't want to talk anyway.")
            dialog("It's not like I had any crucial tips to give.")
            sage.rest()
        end)()
    end
end)
