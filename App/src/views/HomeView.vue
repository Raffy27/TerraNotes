<template>
  <main>
    <Toast />

    <div class="column">
      <div class="title">
        <h1>TerraNotes</h1>
        <h4>v0.0.1 alpha</h4>
      </div>

      <p>To get started, enter your API key and upload your files.</p>

      <Transition name="fade" mode="out-in">
        <div class="error" v-if="errorShown">
          <p>{{ errorMessage }}</p>
          <Button @click="errorShown = false; progressShown = false" label="Back" />
        </div>
        <div class="progress" v-else-if="progressShown">
          <ProgressSpinner stroke-width="3" />
          <p>{{ displayStatus }}</p>
        </div>
        <div class="content" v-else-if="content">
          <pre>{{ content }}</pre>
          <Button @click="content = null" label="Back" />
        </div>
        <UploadForm v-else @submit="formSubmit" />
      </Transition>
    </div>
  </main>
</template>

<script setup lang="ts">
import UploadForm from '@/components/UploadForm.vue';
import Toast from 'primevue/toast';
import ProgressSpinner from 'primevue/progressspinner';
import Button from 'primevue/button';
import { useToast } from 'primevue/usetoast';
import { computed, ref } from 'vue';

const toast = useToast();

const errorShown = ref<boolean>(false);
const errorMessage = ref<string>('');
const progressShown = ref<boolean>(false);
const status = ref<string | null>(null);
const content = ref<string | null>(null);

let pollInterval: number | null = null;

const displayStatus = computed(() => {
  return status.value ? status.value.charAt(0).toUpperCase() + status.value.slice(1) : null;
});

async function formSubmit(apiKey: string, files: File[]) {
  if (apiKey.trim() === '') {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'You must enter an API key.',
      life: 3000,
    });
    return;
  }
  if (files.length === 0) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'You must upload at least one file.',
      life: 3000,
    });
    return;
  }

  progressShown.value = true;
  await transformFiles(apiKey, files);
}

async function transformFiles(apiKey: string, files: File[]) {
  const formData = new FormData();
  files.forEach((file) => formData.append('files', file));
  progressShown.value = true;

  try {
    const response = await fetch('http://localhost:5268/api/Notes', {
      method: 'POST',
      headers: {
        'X-Api-Key': apiKey,
      },
      body: formData,
    });

    if (!response.ok) {
      throw new Error('An error occurred while uploading your files.');
    }

    const data = await response.json();
    status.value = data.status;
    
    pollInterval = setInterval(() => poll(apiKey, data.id), 1000);
  } catch (error) {
    errorShown.value = true;
    errorMessage.value = error + '';
  }
}

async function poll(apiKey: string, id: number) {
  if (progressShown.value) {
    try {
      const response = await fetch(`http://localhost:5268/api/Notes/${id}`, {
        method: 'GET',
        headers: {
          'X-Api-Key': apiKey,
        },
      });

      if (!response.ok) {
        throw new Error('An error occurred while polling for status.');
      }

      const data = await response.json();
      status.value = data.status;
      if (data.status === 'complete') {
        progressShown.value = false;
        content.value = data.content;
        clearInterval(pollInterval!);
        pollInterval = null;
      }
    } catch (error) {
      errorShown.value = true;
      errorMessage.value = error + '';
      clearInterval(pollInterval!);
      pollInterval = null;
    }
  }
}
</script>

<style scoped>
.column {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
.title {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.title h1 {
  font-size: 3.5rem;
  font-weight: 700;
  margin-bottom: 0;
  background: -webkit-linear-gradient(120deg, #db9ee6, #9eade6);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.title h4 {
  margin-top: 0;
  font-weight: 400;
  color: #999;
}

/* Fade effect */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 100ms;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@keyframes p-progress-spinner-color {
  0% {
    stroke: #9eade6;
  }
  50% {
    stroke: #db9ee6;
  }
  100% {
    stroke: #9eade6;
  }
}

:deep(.p-progress-spinner-circle) {
  animation: p-progress-spinner-dash 1.5s ease-in-out infinite, p-progress-spinner-color 6s ease-in-out infinite;
}

.progress {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  row-gap: 1rem;
}
</style>
