# EPG
Generates a scrolling electronic program guide display from an XMLTV schedule


Settings
---------
The settings are all contained in the epg.xml file.  A sample is provided.

Data Sources
---------
The program can read schedules from XMLTV files provided by other sources.  In the system I maintain, I am using EPGCollector to generate XMLTV files for the local over-the-air stations, and I have a script that generates XMLTV files from a DirecTV receiver.
The `channel_id` parameter in the epg.xml matches a channel to the `channel` parameter in a `programme` node.
If you leave the `name` field blank, it will use the `display-name` for the channel from the XMLTV file.

Static text for a channel (when you do not have schedule data) can be displayed by setting the source to "static"
