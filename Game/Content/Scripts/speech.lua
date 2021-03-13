require 'common'

function make_speaker(defaults)
    return function(line, overrides)
        local opts = {}
        opts['line'] = line
        for k,v in pairs(defaults) do
            opts[k] = v
        end
        if overrides ~= nil then
            for k,v in pairs(overrides) do
                opts[k] = v
            end
        end
        return dialog(opts)
    end
end

hal_speak = make_speaker{
    speaker='Hal',
    portrait='default_neutral',
}

dark_lord_speak = make_speaker{
    speaker='Dark Lord',
    portrait='dark_lord_neutral',
    pitch=-0.8,
}

goblin_speak = make_speaker{
    speaker='Henchman',
    portrait='goblin_neutral',
    pitch=0.8,
}
