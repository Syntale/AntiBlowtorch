# Anti Blowtorch
Prevent players from repairing or salvaging structures while being raided.

## Features
- Prevent players from repairing the structure if it was recently damaged.
- Prevent players from salvaging the structure if it was recently damaged.
- Customizable messages and throttle time.

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<AntiBlowtorchConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <MessageIconUrl>https://i.imgur.com/3bYaNFM.png</MessageIconUrl>
  <BlockTimeSeconds>60</BlockTimeSeconds>
  <MessageThrottleTimeSeconds>2</MessageThrottleTimeSeconds>
  <IgnoreOwnerAndGroup>false</IgnoreOwnerAndGroup>
</AntiBlowtorchConfiguration>
```

- `MessageColor` - The color of the chat messages sent by the plugin.
- `MessageIconUrl` - The icon of the chat messages sent by the plugin.
- `BlockTimeSeconds` - The time in seconds that the structure will be blocked from being repaired or salvaged after being damaged.
- `MessageThrottleTimeSeconds` - The time in seconds that the message will be sent again to the player. For example when the player uses blowtorch on the structure, without a throttle time the message will be sent multiple times and spam the player.
- `IgnoreOwnerAndGroup` - If set to `true`, the plugin will ignore the owner and group of the structure and block the repair or salvage action regardless of who damaged the structure.

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="BlockRepair" Value="You can't repair this [[b]]{0}[[/b]], because it was recently damaged. Wait [[b]]{1}[[/b]] seconds." />
  <Translation Id="BlockSalvage" Value="You can't salvage this [[b]]{0}[[/b]], because it was recently damaged. Wait [[b]]{1}[[/b]] seconds." />
</Translations>
```