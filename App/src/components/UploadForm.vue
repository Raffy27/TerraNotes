<template>
    <form class="upload-form">
        <div class="p-fluid">
            <label for="api-key">API Key</label>
            <InputText id="api-key" v-model="apiKey" />
        </div>

        <div class="p-fluid">
            <label for="files">Files (max 5)</label>
            <FileUpload id="files" ref="uploader" name="files[]" accept="application/pdf, image/png, image/jpeg" :file-limit="5" multiple
                auto :show-upload-button="false" :show-cancel-button="false" :max-file-size="10000000"
            />
            <span class="hint">Max 10MB. Accepted formats: pdf, png, jpeg</span>
        </div>

        <div class="p-fluid">
            <Button label="Submit" @click="submit" />
        </div>
    </form>
</template>

<script setup lang="ts">
import InputText from 'primevue/inputtext';
import FileUpload from 'primevue/fileupload';
import Button from 'primevue/button';
import { ref } from 'vue';

const apiKey = ref<string>('');
const uploader = ref();

const emit = defineEmits(['submit']);

function submit() {
    emit('submit', apiKey.value, uploader.value.uploadedFiles);
}
</script>

<style scoped>
.upload-form {
    display: flex;
    flex-direction: column;
    justify-content: center;
    row-gap: 1rem;
}

.hint {
    font-size: 0.8rem;
    color: #999;
    margin-top: 0.5rem;
}
</style>