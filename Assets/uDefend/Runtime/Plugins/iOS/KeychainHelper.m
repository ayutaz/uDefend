/**
 * uDefend iOS Keychain Helper
 * Provides C functions callable from Unity via [DllImport("__Internal")]
 * for storing/retrieving encryption keys in the iOS Keychain.
 */

#import <Foundation/Foundation.h>
#import <Security/Security.h>

/**
 * Store data in the Keychain.
 * Returns 0 on success, -1 on failure.
 */
int uDefend_Keychain_Store(const char* service, const char* account, const uint8_t* data, int dataLength) {
    NSString *serviceStr = [NSString stringWithUTF8String:service];
    NSString *accountStr = [NSString stringWithUTF8String:account];
    NSData *keyData = [NSData dataWithBytes:data length:dataLength];

    // Delete existing item first
    NSDictionary *deleteQuery = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: serviceStr,
        (__bridge id)kSecAttrAccount: accountStr,
    };
    SecItemDelete((__bridge CFDictionaryRef)deleteQuery);

    // Add new item
    NSDictionary *addQuery = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: serviceStr,
        (__bridge id)kSecAttrAccount: accountStr,
        (__bridge id)kSecValueData: keyData,
        (__bridge id)kSecAttrAccessible: (__bridge id)kSecAttrAccessibleAfterFirstUnlock,
    };

    OSStatus status = SecItemAdd((__bridge CFDictionaryRef)addQuery, NULL);
    return (status == errSecSuccess) ? 0 : -1;
}

/**
 * Load data from the Keychain.
 * Returns the number of bytes loaded (>0), or 0 if not found, or -1 on error.
 */
int uDefend_Keychain_Load(const char* service, const char* account, uint8_t* buffer, int bufferLength) {
    NSString *serviceStr = [NSString stringWithUTF8String:service];
    NSString *accountStr = [NSString stringWithUTF8String:account];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: serviceStr,
        (__bridge id)kSecAttrAccount: accountStr,
        (__bridge id)kSecReturnData: @YES,
        (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne,
    };

    CFTypeRef result = NULL;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, &result);

    if (status == errSecSuccess && result != NULL) {
        NSData *data = (__bridge_transfer NSData *)result;
        int copyLength = (int)MIN(data.length, bufferLength);
        memcpy(buffer, data.bytes, copyLength);
        return copyLength;
    }

    if (result != NULL) {
        CFRelease(result);
    }

    return (status == errSecItemNotFound) ? 0 : -1;
}

/**
 * Delete data from the Keychain.
 * Returns 0 on success, -1 on failure.
 */
int uDefend_Keychain_Delete(const char* service, const char* account) {
    NSString *serviceStr = [NSString stringWithUTF8String:service];
    NSString *accountStr = [NSString stringWithUTF8String:account];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: serviceStr,
        (__bridge id)kSecAttrAccount: accountStr,
    };

    OSStatus status = SecItemDelete((__bridge CFDictionaryRef)query);
    return (status == errSecSuccess || status == errSecItemNotFound) ? 0 : -1;
}

/**
 * Check if a key exists in the Keychain.
 * Returns 1 if found, 0 if not.
 */
int uDefend_Keychain_HasKey(const char* service, const char* account) {
    NSString *serviceStr = [NSString stringWithUTF8String:service];
    NSString *accountStr = [NSString stringWithUTF8String:account];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: serviceStr,
        (__bridge id)kSecAttrAccount: accountStr,
    };

    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, NULL);
    return (status == errSecSuccess) ? 1 : 0;
}
