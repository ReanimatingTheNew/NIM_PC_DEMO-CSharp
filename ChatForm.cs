﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NIM;
using System.Runtime.InteropServices;

namespace NIMDemo
{
    public partial class ChatForm : Form
    {
        private readonly string _peerId = null;
        private readonly NIM.Session.NIMSessionType _sessionType;
        private bool _fileDroped = false;
        private nim_vchat_opt_cb_func _createroomcb = null;
        private nim_vchat_opt2_cb_func _joinroomcb = null;
        string room_name = "154145";

        private ChatForm()
        {
            InitializeComponent();
            NIM.TalkAPI.OnReceiveMessageHandler += ReceiveMessageHandler;
            NIM.TalkAPI.OnSendMessageCompleted += SendMessageResultHandler;
            this.FormClosed += new FormClosedEventHandler(ChatForm_FormClosed);
            this.textBox1.DragEnter += TextBox1_DragEnter;
            this.textBox1.DragDrop += TextBox1_DragDrop;

            _createroomcb = new nim_vchat_opt_cb_func(CreateMultiVChatRoomCallback);
            _joinroomcb = new nim_vchat_opt2_cb_func(JoinMultiVChatRoomCallback);
        }

        private void TextBox1_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBox1.Text = path;
            _fileDroped = true;
        }

        private void TextBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        public ChatForm(string id, NIM.Session.NIMSessionType st = NIM.Session.NIMSessionType.kNIMSessionTypeP2P)
            : this()
        {
            _peerId = id;
            room_name = "1554554";
            _sessionType = st;
            base.Text = string.Format("与 {0} 聊天中", _peerId);
            if (st != NIM.Session.NIMSessionType.kNIMSessionTypeP2P)
            {
                testMediaBtn.Visible = false;
                testRtsBtn.Visible = false;
            }
        }

        void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            NIMDemo.Helper.VChatHelper.CurrentVChatType = NIMDemo.Helper.VChatType.kP2P;
            NIM.TalkAPI.OnReceiveMessageHandler -= ReceiveMessageHandler;
            NIM.TalkAPI.OnSendMessageCompleted -= SendMessageResultHandler;
        }

        private void SendMessageToFriend(object sender, EventArgs e)
        {
            string message = this.textBox1.Text;
            if (_fileDroped)
            {
                _fileDroped = false;
                SendFile(message);
            }
            else
            {
                NIM.NIMTextMessage textMsg = new NIM.NIMTextMessage();
                //textMsg.ReceiverID = "936c97544aa946e5b3efa5fd28f493b3";
                textMsg.SessionType = _sessionType;
                textMsg.ReceiverID = _peerId;
                //textMsg.SessionType = NIM.Session.NIMSessionType.kNIMSessionTypeP2P;
                textMsg.TextContent = message;
                textMsg.PushContent = "云信测试消息--*--";
                NIM.TalkAPI.SendMessage(textMsg);
            }
            this.listBox1.Items.Add(message);
            this.textBox1.Text = string.Empty;
        }
        void SendFile(string path)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path);

            if (IsImageFile(path))
            {
                NIM.NIMImageMessage imageMsg = new NIMImageMessage();
                imageMsg.LocalFilePath = path;
                imageMsg.ImageAttachment = new NIMImageAttachment();
                imageMsg.ImageAttachment.DisplayName = fileName;
                imageMsg.ImageAttachment.FileExtension = extension;
                imageMsg.ReceiverID = _peerId;
                imageMsg.SessionType = _sessionType;
                using (var i = Image.FromFile(path))
                {
                    imageMsg.ImageAttachment.Height = i.Height;
                    imageMsg.ImageAttachment.Width = i.Width;
                }
                TalkAPI.SendMessage(imageMsg);
            }
            else
            {
                NIM.NIMFileMessage fileMsg = new NIMFileMessage();
                fileMsg.LocalFilePath = path;
                fileMsg.FileAttachment = new NIMMessageAttachment();
                fileMsg.FileAttachment.DisplayName = fileName;
                fileMsg.FileAttachment.FileExtension = extension;
                fileMsg.ReceiverID = _peerId;
                fileMsg.SessionType = _sessionType;
                NIM.TalkAPI.SendMessage(fileMsg);
            }
        }

        bool IsImageFile(string path)
        {
            int[] imageBytes = { 0x8950, 0xFFD8, 0x4D42 };
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                var b1 = fs.ReadByte();
                var b2 = fs.ReadByte();
                var r = (b1 << 8) + b2;
                return imageBytes.Contains(r);
            }
        }

        void SendMessageResultHandler(object sender, MessageArcEventArgs args)
        {
            if (args.ArcInfo.Response == ResponseCode.kNIMResSuccess || args.ArcInfo.TalkId != _peerId)
                return;
            Action action = () =>
            {
                MessageBox.Show(args.Dump(), "发送失败");
            };
            this.Invoke(action);
        }

        void ReceiveMessageHandler(object sender, NIM.NIMReceiveMessageEventArgs args)
        {
            if (args.Message.MessageContent.SessionType == NIM.Session.NIMSessionType.kNIMSessionTypeP2P && args.Message.MessageContent.SenderID != _peerId)
                return;
            if (args.Message.MessageContent.SessionType == NIM.Session.NIMSessionType.kNIMSessionTypeTeam && args.Message.MessageContent.ReceiverID != _peerId)
                return;
            //处理自定义消息
            if (args.Message != null && args.Message.MessageContent.MessageType == NIMMessageType.kNIMMessageTypeCustom)
            {
                var m = args.Message.MessageContent as NIMCustomMessage<object>;
                //CustomContent 是一个JObject对象，可以根据Extention字段或者其他方法来决定如何解析
                var jObj = m.CustomContent as JObject;
                if (m.Extention == "1")
                {
                    var x = jObj.ToObject<CustomMessageContent>();
                    if (x != null)
                    {

                    }
                }
            }
            if (args.Message != null && args.Message.MessageContent.MessageType == NIM.NIMMessageType.kNIMMessageTypeText)
            {
                Action action = () =>
                {
                    var m = args.Message.MessageContent as NIM.NIMTextMessage;
                    this.listBox1.Items.Add(new string(' ', 50) + m.TextContent);
                };
                this.Invoke(action);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((obj) =>
            {
                VideoTest();
            });
        }

        public void VideoTest()
        {
            NIMVChatInfo info = new NIMVChatInfo();
            info.Uids = new System.Collections.Generic.List<string>();
            info.Uids.Add(_peerId);
            VChatAPI.Start(NIMVideoChatMode.kNIMVideoChatModeVideo, info);//邀请test_id进行语音通话
        }

        private void QueryMsglogBtn_Click(object sender, EventArgs e)
        {
            //var f = new InfomationForm();
            //f.Load += (s, ex) =>
            //{
            //    NIM.Messagelog.MessagelogAPI.QueryMsglogOnline(_peerId, 
            //        _sessionType, 100, 0, 0, 0, false, false,
            //        (a, b, c, d) =>
            //        {
            //            var x = d.Dump();
            //            f.ShowMessage(x);
            //        });
            //};
            //f.Show();
            MessageLogForm form = new MessageLogForm();
            form.Show();
        }

        private void CreateMultiVChatRoomCallback(bool ret, int code,string json_extension,IntPtr user_data)
		{
            if (ret)
            {
                json_extension = "{\"session_id\":\"leewp\"}";
                //创建房间成功,将内容抛至UI线程  
                Action action = () =>
                    {
						if (VChatAPI.JoinRoom(NIMVideoChatMode.kNIMVideoChatModeVideo, room_name, json_extension, _joinroomcb, user_data))
						{
							//调用成功
						}
						else
						{
							string info = String.Format("加入房间-{0}-失败", room_name);
							MessageBox.Show(info);
						}

					};
                    this.Invoke(action);
            }
            else
            {
                Action action = () =>
                {
                    MessageBox.Show("创建房间失败-错误码：" + code.ToString());
                };
                this.BeginInvoke(action);

            }
		}

		private void JoinMultiVChatRoomCallback(int code, Int64 channel_id, string json_extension,IntPtr user_data)
		{
			if (code==200)
			{
                System.Diagnostics.Debug.WriteLine("进入房间：" + json_extension);
                //进入房间成功
                Action action = () =>
                {
                    MultiVChatForm vchat = new MultiVChatForm(room_name);
                    vchat.Show();
                };
                this.Invoke(action);
			}
			else
			{
                Action action = () =>
                {
                    MessageBox.Show("加入房间失败-错误码:" + code.ToString());
                };
                this.BeginInvoke(action);
				
			}
		}
        private void button3_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((obj) =>
            {
                RtsTest();
            });
        }

        public void RtsTest()
        {
            NIM.NIMRts.RtsStartInfo info = new NIM.NIMRts.RtsStartInfo();
            info.ApnsText = "123";
            info.CustomInfo = "456";
            RtsAPI.SetAckNotifyCallback((a, b, c, d) =>
            {
                this.InvokeOnMainThread(() =>
                {
                    if (!c)
                        MessageBox.Show("对方拒绝");
                    else
                    {
                        RtsForm f = new RtsForm(a);
                        f.Show();
                    }
                });
            });
            RtsAPI.Start((NIM.NIMRts.NIMRtsChannelType.kNIMRtsChannelTypeTcp | NIM.NIMRts.NIMRtsChannelType.kNIMRtsChannelTypeVchat), _peerId, info,
                (code, sessionId, channelType, uid) =>
                {
                    this.InvokeOnMainThread(() =>
                    {
                        if (code == 200)
                        {
                            MessageBox.Show("邀请已发送，等待对方加入");
                        }
                        else
                        {
                            MessageBox.Show("邀请失败:" + ((NIM.ResponseCode)code).ToString());
                        }
                    });
                });
        }

        private void SysMsgBtn_Click(object sender, EventArgs e)
        {
            NIM.SysMessage.NIMSysMessageContent content = new NIM.SysMessage.NIMSysMessageContent();
            content.ClientMsgId = Guid.NewGuid().ToString();
            content.ReceiverId = _peerId;
            if (_sessionType == NIM.Session.NIMSessionType.kNIMSessionTypeP2P)
                content.MsgType = NIM.SysMessage.NIMSysMsgType.kNIMSysMsgTypeCustomP2PMsg;
            else if (_sessionType == NIM.Session.NIMSessionType.kNIMSessionTypeTeam)
                content.MsgType = NIM.SysMessage.NIMSysMsgType.kNIMSysMsgTypeCustomTeamMsg;
            content.Attachment = textBox1.Text;
            NIM.SysMessage.SysMsgAPI.SendCustomMessage(content);

            //发送自定义消息
            //ImageTextMessage msg = new ImageTextMessage();
            //msg.CustomContent.Text = "这是一条图文消息:hello nim";
            //msg.LocalFilePath = @"C:\Users\鹏\Pictures\13349232980.jpg";
            //msg.ReceiverID = _peerId;
            //NIM.Nos.NosAPI.Upload(msg.LocalFilePath, (a, b) =>
            //{
            //    msg.CustomContent.ImagePath = b;
            //    NIM.TalkAPI.SendMessage(msg);
            //}, null);
        }

        private void SysmsgLogBtn_Click(object sender, EventArgs e)
        {
            NIM.SysMessage.SysMsgAPI.QueryMessage(100, 0, (r) =>
            {
                Action action = () =>
                {
                    InfomationForm f = new InfomationForm();
                    f.Load += (s, ex) =>
                    {
                        var x = r.Dump();
                        f.ShowMessage(x);
                    };
                    f.Show();
                };
                this.Invoke(action);
            });
        }

        void InvokeOnMainThread(Action action)
        {
            this.BeginInvoke(action);
        }

        private void btn_createmultiroom_Click(object sender, EventArgs e)
        {
            NIMDemo.Helper.VChatHelper.CurrentVChatType = NIMDemo.Helper.VChatType.kMulti;
            string custom_info = "custom_info";
            string json_extension = "";
            room_name = Guid.NewGuid().ToString("N");

			//
            VChatAPI.CreateRoom(room_name, custom_info, json_extension, _createroomcb, IntPtr.Zero);
        }

        private void btn_joinmultiroom_Click(object sender, EventArgs e)
        {
            InputRoomIdForm form = new InputRoomIdForm();
            form.ShowDialog();
        }
    }

    class CustomMessageContent
    {
        [Newtonsoft.Json.JsonProperty("text")]
        public string Text { get; set; }

        [Newtonsoft.Json.JsonProperty("imagepath")]
        public string ImagePath { get; set; }
    }

    class ImageTextMessage : NIMCustomMessage<CustomMessageContent>
    {
        public override CustomMessageContent CustomContent { get; set; }

        public ImageTextMessage()
        {
            MessageType = NIMMessageType.kNIMMessageTypeCustom;
            CustomContent = new CustomMessageContent();
            Extention = "1";
        }
    }

   
}