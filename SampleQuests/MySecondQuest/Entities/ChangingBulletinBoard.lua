-- ChangingBulletinBoard.lua
-- This entity is a "Reader" of global state. It is not behavioral.

-- We use this local variable to track the last known state of the global flag.
-- This prevents us from calling SetReadableText on every single frame, which is inefficient.
local lastKnownState = nil

function Init(quest, me)
    quest:Log("ChangingBulletinBoard '" .. me:GetDataString() .. "' initialized.")
end

function Main(quest, me)

    while true do
        local isChocolateGiven = quest:GetGlobalBool("BanditGaveChocolate")

        if isChocolateGiven ~= lastKnownState then
            quest:Log("ChangingBulletinBoard: Global flag 'BanditGaveChocolate' has changed! Updating text.")

            if isChocolateGiven then
                me:SetReadableText("TEXT_DATABASE_ENTRY_CHOCOLATE_GIVEN") 
            else
                me:SetReadableText("TEXT_DATABASE_ENTRY_CHOCOLATE_NOT_GIVEN")
            end

            lastKnownState = isChocolateGiven
        end

        if not quest:Wait(0) then
            break 
        end
    end
end
