package com.luigivincent.presentationcontroller;

import android.content.Context;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

import com.getpebble.android.kit.PebbleKit;
import com.getpebble.android.kit.PebbleKit.PebbleDataReceiver;
import com.getpebble.android.kit.util.PebbleDictionary;

import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

public class ControlActivity extends AppCompatActivity {
    private static final String TAG = "ControlActivity";
    private static final UUID APP_ID = UUID.fromString("79a703ed-ddda-4ea2-bc99-4ef3914ce017");
    private PrintWriter out;
    private Socket socket;
    private static final Map<Integer, String> COMMANDS;

    static {
        Map<Integer, String> commandMap = new HashMap<>();
        commandMap.put(R.id.rightButton, "right");
        commandMap.put(R.id.leftButton, "left");
        commandMap.put(R.id.stopButton, "die");
        COMMANDS = Collections.unmodifiableMap(commandMap);
    }

    /**
     * The object to receive, acknowledge and process data from the companion Pebble WatchApp.
     */
    PebbleDataReceiver dataReceiver = new PebbleDataReceiver(APP_ID) {
        @Override
        public void receiveData(Context context, int id, PebbleDictionary dictionary) {
            PebbleKit.sendAckToPebble(context, id);
            for (PebbleTrigger trigger : PebbleTrigger.values()) {
                if (dictionary.getInteger(trigger.key()) != null) {
                    send(trigger.action());
                    break;
                }
            }
        }
    };

    @Override
    public void onResume() {
        super.onResume();
        PebbleKit.registerReceivedDataHandler(getApplicationContext(), dataReceiver);
        PebbleKit.startAppOnPebble(getApplicationContext(), APP_ID);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_control);
        String ip = getIntent().getStringExtra("ip");
        connect(ip, 9001);
    }

    private void connect(final String host, final int port) {
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    socket = new Socket(host, port);
                    out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream()), true);
                } catch (IOException ex) {
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(ControlActivity.this, "Could not connect. Port: " + port + ", host: " + host, Toast.LENGTH_SHORT).show();
                        }
                    });
                    debug("Could not connect.", ex);
                }
            }
        }).start();
    }

    public void executeCommand(View v) {
        send(COMMANDS.get(v.getId()));
    }

    private void send(final String message) {
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    out.print(message);
                    out.flush();
                } catch (NullPointerException ex) {
                    debug("Could not send.", ex);
                }
            }
        }).start();
    }

    private void debug(String message, Exception reason) {
        Log.d(TAG, message + ' ' + reason.getMessage());
    }
}
