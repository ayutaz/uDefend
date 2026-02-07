package com.udefend;

import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import java.security.KeyStore;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;
import android.util.Base64;
import android.content.Context;
import android.content.SharedPreferences;
import com.unity3d.player.UnityPlayer;

/**
 * Helper class for storing and retrieving encryption keys using Android Keystore.
 * Uses AES-256-GCM with hardware-backed keystore when available.
 */
public class KeystoreHelper {

    private static final String KEYSTORE_PROVIDER = "AndroidKeyStore";
    private static final String PREFS_NAME = "udefend_keys";
    private static final String TRANSFORM = "AES/GCM/NoPadding";
    private static final int GCM_TAG_LENGTH = 128;

    /**
     * Stores a key by encrypting it with a Keystore-backed AES key.
     */
    public static void storeKey(String alias, byte[] keyData) {
        try {
            SecretKey keystoreKey = getOrCreateKeystoreKey(alias);
            Cipher cipher = Cipher.getInstance(TRANSFORM);
            cipher.init(Cipher.ENCRYPT_MODE, keystoreKey);

            byte[] iv = cipher.getIV();
            byte[] encrypted = cipher.doFinal(keyData);

            // Store IV + encrypted data as Base64
            byte[] combined = new byte[iv.length + encrypted.length];
            System.arraycopy(iv, 0, combined, 0, iv.length);
            System.arraycopy(encrypted, 0, combined, iv.length, encrypted.length);

            String encoded = Base64.encodeToString(combined, Base64.NO_WRAP);
            getPrefs().edit().putString(alias, encoded).apply();
        } catch (Exception e) {
            throw new RuntimeException("Failed to store key: " + e.getMessage(), e);
        }
    }

    /**
     * Retrieves and decrypts a previously stored key.
     */
    public static byte[] getKey(String alias) {
        try {
            String encoded = getPrefs().getString(alias, null);
            if (encoded == null) return null;

            byte[] combined = Base64.decode(encoded, Base64.NO_WRAP);
            byte[] iv = new byte[12]; // GCM standard IV size
            byte[] encrypted = new byte[combined.length - 12];
            System.arraycopy(combined, 0, iv, 0, 12);
            System.arraycopy(combined, 12, encrypted, 0, encrypted.length);

            KeyStore keyStore = KeyStore.getInstance(KEYSTORE_PROVIDER);
            keyStore.load(null);
            SecretKey keystoreKey = (SecretKey) keyStore.getKey(alias, null);

            Cipher cipher = Cipher.getInstance(TRANSFORM);
            cipher.init(Cipher.DECRYPT_MODE, keystoreKey, new GCMParameterSpec(GCM_TAG_LENGTH, iv));
            return cipher.doFinal(encrypted);
        } catch (Exception e) {
            throw new RuntimeException("Failed to retrieve key: " + e.getMessage(), e);
        }
    }

    /**
     * Checks if a key exists.
     */
    public static boolean hasKey(String alias) {
        return getPrefs().contains(alias);
    }

    /**
     * Deletes a stored key and its Keystore entry.
     */
    public static void deleteKey(String alias) {
        try {
            getPrefs().edit().remove(alias).apply();
            KeyStore keyStore = KeyStore.getInstance(KEYSTORE_PROVIDER);
            keyStore.load(null);
            if (keyStore.containsAlias(alias)) {
                keyStore.deleteEntry(alias);
            }
        } catch (Exception e) {
            throw new RuntimeException("Failed to delete key: " + e.getMessage(), e);
        }
    }

    private static SecretKey getOrCreateKeystoreKey(String alias) throws Exception {
        KeyStore keyStore = KeyStore.getInstance(KEYSTORE_PROVIDER);
        keyStore.load(null);

        if (keyStore.containsAlias(alias)) {
            return (SecretKey) keyStore.getKey(alias, null);
        }

        KeyGenParameterSpec spec = new KeyGenParameterSpec.Builder(
                alias,
                KeyProperties.PURPOSE_ENCRYPT | KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setKeySize(256)
                .build();

        KeyGenerator generator = KeyGenerator.getInstance(
                KeyProperties.KEY_ALGORITHM_AES, KEYSTORE_PROVIDER);
        generator.init(spec);
        return generator.generateKey();
    }

    private static SharedPreferences getPrefs() {
        Context context = UnityPlayer.currentActivity.getApplicationContext();
        return context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
    }
}
