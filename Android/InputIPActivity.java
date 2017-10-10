package com.luigivincent.presentationcontroller;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;

public class InputIPActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_input_ip);
    }

    public void connect(View v) {
        EditText ipText = (EditText) findViewById(R.id.ipText);
        Intent intent = new Intent(getBaseContext(), ControlActivity.class);
        intent.putExtra("ip", ipText.getText().toString());
        startActivity(intent);
    }
}