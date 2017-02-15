const fs = require('fs');
const Path = require('path');
const Discord = require('discord.js');
const EventDispacher = require(Path.resolve(__dirname, '../utilities/EventDispacher.js'));
const Message = require(Path.resolve(__dirname, './Message.js'));

module.exports = function (token) {
  const mapper = new WeakMap();
  const client = new Discord.Client();

  // Internals
  /* Init */
  client.on('ready', () => {
    if (!this.isReady) {
      this.isReady = true;
    }

    console.log(this.getName() + ' service is ready!');
  });

  client.on('message', (message) => {
    if (message.author.id === client.user.id) {
      return;
    }

    this.onMessageReceived.dispach(parse(message));
  });

  client.login(token);

  /* Message specific */

  const parse = (source) => {
    const message = new Message(
      source.author.username,
      source.content
    );

    mapper.set(message, source);

    message.internals = {
      service: this,
      author: source.author.id
    };

    return message;
  };

  const format = {
    asBlock: (text) => '`' + text + '`',
    asItalics: (text) => '*' + text + '*',
    asBold: (text) => '**' + text + '**',
    asMultiline: (text) => '```' + text + '```'
  };

  /* Audio specific */

  const getAudioChannel = (name) => {
    return client.channels.find((channel) => channel.type === 'voice' && channel.name === name);
  };

  const getFirstAudioChannel = (message) => {
    return message.guild.channels.find((channel) => channel.type === 'voice');
  };

  const audioInterface = {
    path: Path.resolve(__dirname, '../sounds'),
    play: function (message, filename, channel) {
      const msg = mapper.get(message);
      const path = Path.join(this.path, filename + '.mp3');

      if (!fs.existsSync(path)) {
        return false; // File does not exist
      }

      if (client.voiceConnections.first() !== undefined) {
        return false; // Already playing something on this channel
      }

      // If there is not specified channel, choose one that user is connected to
      if (channel === undefined) {
        channel = msg.member.voiceChannel;

        // If user is not connected to any channel, use default
        if (channel === undefined) {
          channel = getFirstAudioChannel(msg);
        }

      // else get VoiceChannel object from it's name
      } else {
        channel = getAudioChannel(channel);

        if (channel === null) {
          return false; // There is no channel like that
        }
      }

      channel.join()
        .then((connection) => {
          const dipacher = connection.playFile(path);

          function disconnect (event, listener) {
            connection.disconnect();
          }

          dipacher.once('end', disconnect);
          dipacher.once('error', disconnect);
        });

      return true;
    }
  };

  // Public interface

  /* Service specific */

  this.isReady = false;

  this.getName = () => 'Discord#' + (this.isReady ? client.user.username : 'unknown');

  this.getChatRooms = () => this.isReady ? client.guilds.map((entry) => entry.toString()) : undefined;

  this.getFormatting = () => format;

  /* Message specific */

  this.onMessageReceived = new EventDispacher();

  this.dispose = (message) => mapper.delete(message);

  this.reply = (originalMessage, replyContent, deleteOriginal) => {
    const discordMsg = mapper.get(originalMessage);

    if (deleteOriginal === undefined || deleteOriginal === true) {
      discordMsg.delete();
    }

    return new Promise((resolve, reject) => {
      discordMsg.channel.send(replyContent)
        .then((msg) => resolve(parse(msg)))
        .catch((err) => reject(err));
    });
  };

  this.edit = (message) => {
    return new Promise((resolve, reject) => {
      mapper.get(message).edit(message.content)
        .then((msg) => resolve(message))
        .catch((err) => reject(err));
    });
  };

  this.delete = (message) => {
    return new Promise((resolve, reject) => {
      mapper.get(message).delete()
        .then((msg) => resolve())
        .catch((err) => reject(err));
    });
  };

  /* Audio specific */
  this.getAudioInterface = () => audioInterface;
};
